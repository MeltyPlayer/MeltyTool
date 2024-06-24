using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.io;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.schema.matrix;
using fin.util.hash;

using visceral.schema.geo;
using visceral.schema.mtlb;
using visceral.schema.rcb;

namespace visceral.api {
  public class GeoModelImporter : IModelImporter<GeoModelFileBundle> {
    public const bool STRICT_DAT = false;

    public IModel Import(GeoModelFileBundle modelFileBundle) {
      var files = modelFileBundle.Files.ToHashSet();
      var finModel = new ModelImpl {
          FileBundle = modelFileBundle,
          Files = files,
      };

      // Builds skeletons
      IBone[] finBones = Array.Empty<IBone>();
      var rcbFile = modelFileBundle.RcbFile;
      if (rcbFile != null) {
        this.AddRcbFileToModel_(finModel, rcbFile, out finBones);
      }

      foreach (var bnkFile in modelFileBundle.BnkFiles) {
        new BnkReader().ReadBnk(finModel, bnkFile, rcbFile, finBones);
      }

      // Gets materials
      var mtlbFileIdsDictionary = modelFileBundle.MtlbFileIdsDictionary;
      var tg4hFileIdDictionary = modelFileBundle.Tg4hFileIdDictionary;

      var tg4ImageReader = new Tg4ImageReader();
      var lazyTextureByBundleDictionary
          = new LazyDictionary<Tg4ImageFileBundle, ITexture>(
              bundle => {
                var image = tg4ImageReader.ReadImage(bundle);
                var finTexture = finModel.MaterialManager.CreateTexture(image);
                finTexture.Name = bundle.Tg4hFile.NameWithoutExtension;

                // TODO: How is this set??
                finTexture.WrapModeU = WrapMode.REPEAT;
                finTexture.WrapModeV = WrapMode.REPEAT;

                return finTexture;
              });
      var lazyTextureByIdDictionary = new LazyDictionary<uint, ITexture>(
          id => {
            var tg4hFile = tg4hFileIdDictionary[id];
            var tg4dFile
                = new FinFile(tg4hFile.FullNameWithoutExtension + ".tg4d");
            files.Add(tg4hFile);
            files.Add(tg4dFile);

            return lazyTextureByBundleDictionary[new Tg4ImageFileBundle {
                Tg4hFile = tg4hFile,
                Tg4dFile = tg4dFile,
            }];
          });
      var lazyTextureByChannelDictionary
          = new LazyDictionary<MtlbChannel?, ITexture?>(
              channel => {
                if (channel == null) {
                  return null;
                }

                return lazyTextureByIdDictionary[(uint) channel.IdValues![1]];
                //return lazyTextureByPathDictionary[channel.Path];
              });
      var lazyMtlbDictionary = new LazyDictionary<uint, IReadOnlyList<Mtlb>>(
          mtlbId =>
              mtlbFileIdsDictionary[mtlbId]
                  .Select(mtlbFile => mtlbFile.ReadNew<Mtlb>())
                  .ToArray());
      var lazyMaterialDictionary = new LazyDictionary<uint, IMaterial>(
          mtlbId => {
            var mtlb = lazyMtlbDictionary[mtlbId].First();

            var samplerChannels =
                mtlb.HighLodMaterialChannels
                    .Where(c => c.MtlbChannelCategory ==
                                MtlbChannelCategory.Sampler)
                    .Where(c => c.Path != "(null)" && c.Path.Contains('.'))
                    .DistinctBy(c => FluentHash.Start()
                                               .With(c.Type)
                                               .With(c.Path))
                    .ToArray();

            var material
                = finModel.MaterialManager.AddStandardMaterial();
            material.DiffuseTexture = lazyTextureByChannelDictionary[
                samplerChannels.SingleOrDefault(
                    channel => channel.Type ==
                               MtlbChannelType.DiffuseSampler)];
            material.NormalTexture = lazyTextureByChannelDictionary[
                samplerChannels.SingleOrDefault(
                    channel => channel.Type ==
                               MtlbChannelType.NormalSampler)];
            material.AmbientOcclusionTexture = lazyTextureByChannelDictionary[
                samplerChannels.SingleOrDefault(
                    channel => channel.Type ==
                               MtlbChannelType.OcclusionSampler)];
            material.SpecularTexture = lazyTextureByChannelDictionary[
                samplerChannels.SingleOrDefault(
                    channel => channel.Type ==
                               MtlbChannelType.SpecularTexSampler)];
            material.EmissiveTexture = lazyTextureByChannelDictionary[
                samplerChannels.SingleOrDefault(
                    channel => channel.Type ==
                               MtlbChannelType.EmissiveSampler)];

            material.Name = mtlb.Name;

            return material;
          });

      // Builds meshes
      var geoFiles = modelFileBundle.GeoFiles;
      foreach (var geoFile in geoFiles) {
        this.AddGeoFileToModel_(finModel,
                                geoFile,
                                finBones,
                                lazyMaterialDictionary);
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

          var finBone = finParentBone.AddChild(currentMatrix);
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
        IReadOnlyFinDictionary<uint, IMaterial> materialDictionary) {
      var geo = geoFile.ReadNew<Geo>();

      foreach (var geoBone in geo.Bones) {
        finBones[geoBone.Id].Name = geoBone.Name;
      }

      var finSkin = finModel.Skin;
      foreach (var geoMesh in geo.Meshes) {
        var finMesh = finSkin.AddMesh();
        finMesh.Name = geoMesh.Name;

        var finMaterial = materialDictionary[geoMesh.MtlbId];

        var finVertices =
            geoMesh
                .Vertices
                .Select(
                    geoVertex => {
                      var vertex = finSkin.AddVertex(geoVertex.Position);

                      var boneWeights =
                          geoVertex.Weights
                                   .Select((weight, i)
                                               => (geoVertex.Bones[i], weight))
                                   .Where(boneWeight => boneWeight.weight > 0)
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
               .SetMaterial(finMaterial)
               .SetVertexOrder(VertexOrder.NORMAL);
      }
    }

    public IFinMatrix4x4 GetMatrixFromBone_(Matrix4x4f matrix)
      => new FinMatrix4x4(matrix.Values.AsSpan()).InvertInPlace();
  }
}