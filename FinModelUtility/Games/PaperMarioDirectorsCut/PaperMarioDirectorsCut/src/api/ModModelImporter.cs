using fin.image;
using fin.io;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using pmdc.schema.mod;

namespace pmdc.api {
  public class ModModelFileBundle : IModelFileBundle {
    public required string GameName { get; init; }

    public required IReadOnlyTreeFile ModFile { get; init; }
    public IReadOnlyTreeFile MainFile => this.ModFile;
  }

  public class ModModelImporter : IModelImporter<ModModelFileBundle> {
    public IModel Import(ModModelFileBundle modelFileBundle) {
      var modFile = modelFileBundle.ModFile;
      var mod = modFile.ReadNewFromText<Mod>();

      var files = modFile.AsFileSet();
      var finModel = new ModelImpl<NormalUvVertexImpl>(
          (index, position) => new NormalUvVertexImpl(index, position)) {
          FileBundle = modelFileBundle,
          Files = files
      };

      CreateAdjustedRootBone(finModel, out var finRoot);
      AddToModel(mod, finModel, finRoot, out _, out _);

      return finModel;
    }

    public static void CreateAdjustedRootBone(IModel finModel, out IBone bone) {
      var finSkeleton = finModel.Skeleton;
      bone = finSkeleton.Root.AddRoot(0, 0, 0);
      bone.LocalTransform.SetRotationDegrees(-90, 180, 0);
      bone.LocalTransform.SetScale(-1, 1, 1);
    }

    public static void AddToModel(Mod mod,
                                  IModel<ISkin<NormalUvVertexImpl>> finModel,
                                  IReadOnlyBone bone,
                                  out IMesh finMesh,
                                  out IPrimitive finPrimitive) {
      var finSkin = finModel.Skin;
      finMesh = finSkin.AddMesh();

      var boneWeights = finSkin.GetOrCreateBoneWeights(
          VertexSpace.RELATIVE_TO_BONE,
          bone);

      var finVertices
          = mod.Vertices
               .Where(omdVertex => omdVertex.Something == 8)
               .Select(modVertex => {
                 var finVertex = finSkin.AddVertex(modVertex.Position);
                 finVertex.SetLocalNormal(-modVertex.Normal);
                 finVertex.SetUv(modVertex.Uv);
                 finVertex.SetBoneWeights(boneWeights);

                 return finVertex;
               })
               .ToArray();

      finPrimitive = finMesh.AddTriangles(finVertices);
    }
  }
}