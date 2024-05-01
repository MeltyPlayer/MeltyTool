using System.Drawing;

using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.io;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.schema.matrix;

using visceral.schema.geo;
using visceral.schema.mtlb;
using visceral.schema.rcb;

namespace visceral.api {
  public class GeoModelImporter : IModelImporter<GeoModelFileBundle> {
    public const bool STRICT_DAT = false;

    public IModel Import(GeoModelFileBundle modelFileBundle) {
      var finModel = new ModelImpl();

      // Builds skeletons
      IBone[] finBones = Array.Empty<IBone>();
      var rcbFile = modelFileBundle.RcbFile;
      if (rcbFile != null) {
        this.AddRcbFileToModel_(finModel, rcbFile, out finBones);
      }

      foreach (var bnkFile in modelFileBundle.BnkFiles) {
        new BnkReader().ReadBnk(finModel, bnkFile, rcbFile, finBones);
      }

      // Builds textures
      var textureBundles = modelFileBundle.Tg4ImageFileBundles;
      var textures = new List<ITexture>();
      if (textureBundles != null) {
        var tg4ImageReader = new Tg4ImageReader();

        foreach (var textureBundle in textureBundles) {
          var image = tg4ImageReader.ReadImage(textureBundle);
          var finTexture = finModel.MaterialManager.CreateTexture(image);
          finTexture.Name = textureBundle.Tg4hFile.NameWithoutExtension;

          textures.Add(finTexture);
        }
      }

      // Gets materials
      var lazyMtlbDictionary = new LazyDictionary<uint, IReadOnlyList<Mtlb>>(
          mtlbId =>
              modelFileBundle.MtlbFileIdsDictionary[mtlbId]
                             .Select(mtlbFile => mtlbFile.ReadNew<Mtlb>())
                             .ToArray());
      var lazyMaterialDictionary = new LazyDictionary<uint, IMaterial>(
          mtlbId => {
            var mtlb = lazyMtlbDictionary[mtlbId].First();
            return null!;
          });

      // Builds meshes
      var geoFiles = modelFileBundle.GeoFiles;
      foreach (var geoFile in geoFiles) {
        this.AddGeoFileToModel_(finModel, geoFile, finBones, textures);
      }

      return finModel;
    }

    private void AddRcbFileToModel_(
        IModel finModel,
        IReadOnlyGenericFile rcbFile,
        out IBone[] finBones) {
      finBones = Array.Empty<IBone>();

      var rcb = rcbFile.ReadNew<Rcb>();
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

    private void AddGeoFileToModel_(
        ModelImpl finModel,
        IReadOnlyTreeFile geoFile,
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
        material.Name = baseTextureName;
      } else {
        material = finModel.MaterialManager.AddColorMaterial(Color.Magenta);
      }

      var geo = geoFile.ReadNew<Geo>();

      foreach (var geoBone in geo.Bones) {
        finBones[geoBone.Id].Name = geoBone.Name;
      }

      var finSkin = finModel.Skin;
      foreach (var geoMesh in geo.Meshes) {
        var finMesh = finSkin.AddMesh();
        finMesh.Name = geoMesh.Name;

        var finVertices =
            geoMesh.Vertices
                   .Select(geoVertex => {
                             var vertex = finSkin.AddVertex(geoVertex.Position);

                             var boneWeights =
                                 geoVertex.Weights
                                          .Select((weight, i)
                                                      => (geoVertex.Bones[i],
                                                        weight))
                                          .Where(boneWeight
                                                     => boneWeight.weight > 0)
                                          .Select(boneWeight
                                                      => new BoneWeight(
                                                          finBones[
                                                              geo.Bones[
                                                                      boneWeight
                                                                          .Item1]
                                                                  .Id],
                                                          null,
                                                          boneWeight.Item2))
                                          .ToArray();

                             vertex.SetBoneWeights(
                                 finSkin.GetOrCreateBoneWeights(
                                     VertexSpace.RELATIVE_TO_WORLD,
                                     boneWeights));

                             vertex.SetLocalNormal(geoVertex.Normal);
                             vertex.SetLocalTangent(geoVertex.Tangent);
                             vertex.SetUv(geoVertex.Uv);
                             return vertex as IReadOnlyVertex;
                           })
                   .ToArray();

        var triangles = geoMesh.Faces.Select(geoFace => {
                                               var indices = geoFace.Indices
                                                   .Select(
                                                       index => index -
                                                           geoMesh
                                                               .BaseVertexIndex)
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
      => new FinMatrix4x4(matrix.Values.AsSpan()).InvertInPlace();
  }
}