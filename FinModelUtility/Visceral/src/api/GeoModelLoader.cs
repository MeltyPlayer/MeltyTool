using fin.data;
using fin.data.queue;
using fin.io;
using fin.math;
using fin.math.matrix;
using fin.model;
using fin.model.impl;
using fin.schema.matrix;
using fin.util.enumerables;

using visceral.schema.geo;
using visceral.schema.rcb;


namespace visceral.api {
  public class GeoModelFileBundle : IModelFileBundle {
    // TODO: Is there a better thing to rely on?
    public required string GameName { get; init; }

    public IFileHierarchyFile? MainFile
      => this.RcbFile ?? this.GeoFiles.First();

    public IEnumerable<IGenericFile> Files => this.GeoFiles
                                                  .ConcatIfNonnull(this.RcbFile)
                                                  .ConcatIfNonnull(
                                                      this.Tg4ImageFileBundles
                                                          ?.SelectMany(
                                                              tg4Bundle
                                                                  => new
                                                                      IGenericFile
                                                                      [] {
                                                                          tg4Bundle
                                                                              .Tg4hFile,
                                                                          tg4Bundle
                                                                              .Tg4dFile
                                                                      }));

    public required IReadOnlyList<IFileHierarchyFile> GeoFiles { get; init; }
    public required IFileHierarchyFile? RcbFile { get; init; }

    public IReadOnlyList<Tg4ImageFileBundle>? Tg4ImageFileBundles { get; init; }
  }

  public class GeoModelLoader : IModelLoader<GeoModelFileBundle> {
    public IModel LoadModel(GeoModelFileBundle modelFileBundle) {
      var finModel = new ModelImpl();

      // Builds textures
      var textureBundles = modelFileBundle.Tg4ImageFileBundles;
      var textures = new List<ITexture>();
      if (textureBundles != null) {
        var tg4ImageLoader = new Tg4ImageLoader();

        foreach (var textureBundle in textureBundles) {
          var image = tg4ImageLoader.LoadImage(textureBundle);
          var finTexture = finModel.MaterialManager.CreateTexture(image);
          finTexture.Name = textureBundle.Tg4hFile.NameWithoutExtension;

          textures.Add(finTexture);
        }
      }

      // Builds skeletons
      IBone[] finBones = Array.Empty<IBone>();
      var rcbFile = modelFileBundle.RcbFile;
      if (rcbFile != null) {
        AddRcbFileToModel_(finModel, rcbFile, out finBones);
      }

      // Builds meshes
      var geoFiles = modelFileBundle.GeoFiles;
      foreach (var geoFile in geoFiles) {
        this.AddGeoFileToModel_(finModel, geoFile, finBones, textures);
      }

      return finModel;
    }

    private void AddRcbFileToModel_(
        IModel finModel,
        IFileHierarchyFile rcbFile,
        out IBone[] finBones) {
      finBones = Array.Empty<IBone>();

      var rcb = rcbFile.Impl.ReadNew<Rcb>();
      foreach (var rcbSkeleton in rcb.Skeletons) {
        finBones = new IBone[rcbSkeleton.Bones.Count];

        var finRoot = finModel.Skeleton.Root.AddRoot(0, 0, 0);
        finRoot.Name = rcbSkeleton.SkeletonName;

        var rootChildren = new List<int>();
        var childIndices = new ListDictionary<int, int>();
        for (var i = 0; i < rcbSkeleton.BoneParentIdMap.Count; ++i) {
          var parent = rcbSkeleton.BoneParentIdMap[i];
          if (parent == -1) {
            rootChildren.Add(i);
          } else {
            childIndices.Add(parent, i);
          }
        }

        var boneQueue =
            new FinTuple2Queue<IBone, int>(
                rootChildren.Select(id => (finRoot, id)));
        while (boneQueue.TryDequeue(out var finParentBone, out var id)) {
          var parentId = rcbSkeleton.BoneParentIdMap[id];
          var rcbBone = rcbSkeleton.Bones[id];

          var currentMatrix = GetMatrixFromBone_(rcbBone.Matrix);
          if (parentId != -1) {
            var rcbParentBone = rcbSkeleton.Bones[parentId];
            var parentMatrix = GetMatrixFromBone_(rcbParentBone.Matrix);
            currentMatrix = parentMatrix.InvertInPlace()
                                        .CloneAndMultiply(currentMatrix);
          }

          currentMatrix.Decompose(out var translation,
                                  out var rotation,
                                  out var scale);
          var eulerRadians = QuaternionUtil.ToEulerRadians(rotation);

          var finBone =
              finParentBone
                  .AddChild(translation.X, translation.Y, translation.Z)
                  .SetLocalRotationRadians(
                      eulerRadians.X,
                      eulerRadians.Y,
                      eulerRadians.Z)
                  .SetLocalScale(scale.X, scale.Y, scale.Z);
          finBones[id] = finBone;

          if (childIndices.TryGetList(id, out var currentChildren)) {
            boneQueue.Enqueue(
                currentChildren!.Select(childId => (finBone, childId)));
          }
        }
      }
    }

    private void AddGeoFileToModel_(IModel finModel,
                                    IFileHierarchyFile geoFile,
                                    IBone[] finBones,
                                    IList<ITexture> textures) {
      var colorTextures = textures.Where(texture => texture.Name.EndsWith("_c"))
                                  .ToArray();
      IMaterial material;
      if (colorTextures.Length == 1) {
        var colorTexture = colorTextures[0];
        var baseTextureName = colorTexture.Name[..^2];
        var normalTexture =
            textures.SingleOrDefault(
                texture => texture.Name == $"{baseTextureName}_n");

        var standardMaterial = finModel.MaterialManager.AddStandardMaterial();
        standardMaterial.DiffuseTexture = colorTexture;
        standardMaterial.NormalTexture = normalTexture;
        material = standardMaterial;
      } else {
        material = finModel.MaterialManager.AddNullMaterial();
      }

      var geo = geoFile.ReadNew<Geo>();

      foreach (var geoBone in geo.Bones) {
        finBones[geoBone.Id].Name = geoBone.Name;
      }

      var finSkin = finModel.Skin;
      foreach (var geoMesh in geo.Meshes) {
        var finMesh = finSkin.AddMesh();
        finMesh.Name = geoMesh.Name;

        var finVertices = geoMesh.Vertices
                                 .Select(geoVertex
                                             => finSkin.AddVertex(geoVertex.Position)
                                                 .SetLocalNormal(geoVertex.Normal)
                                                 .SetLocalTangent(geoVertex.Tangent)
                                                 .SetUv(geoVertex.Uv))
                                 .ToArray();

        var triangles = geoMesh.Faces.Select(geoFace => {
                                 var indices = geoFace.Indices
                                     .Select(
                                         index => index -
                                                  geoMesh.BaseVertexIndex)
                                     .ToArray();
                                 return (finVertices[indices[0]],
                                         finVertices[indices[1]],
                                         finVertices[indices[2]]);
                               })
                               .ToArray();

        finMesh.AddTriangles(triangles)
               .SetMaterial(material)
               .SetVertexOrder(VertexOrder.NORMAL);
      }
    }

    public IFinMatrix4x4 GetMatrixFromBone_(Matrix4x4f matrix)
      => new FinMatrix4x4(matrix.Values).InvertInPlace();
  }
}