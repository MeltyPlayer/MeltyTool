using System.Numerics;

using fin.io;
using fin.io.bundles;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using gm.schema.d3d;

namespace gm.api;

public class D3dModelFileBundle : IModelFileBundle {
  public required string GameName { get; init; }

  public required IReadOnlyTreeFile ModFile { get; init; }
  public IReadOnlyTreeFile MainFile => this.ModFile;
}

public class D3dModelImporter : IModelImporter<D3dModelFileBundle> {
  public IModel Import(D3dModelFileBundle modelFileBundle) {
    var modFile = modelFileBundle.ModFile;
    var mod = modFile.ReadNewFromText<D3d>();

    var (finModel, finRootBone)
        = CreateModel((modelFileBundle, modFile.AsFileSet()));
    AddToModel(mod, finModel, finRootBone, out _);

    return finModel;
  }

  public static (IModel<ISkin<NormalUvVertexImpl>>, IBone) CreateModel(
      (IFileBundle fileBundle, IReadOnlySet<IReadOnlyGenericFile> files)?
          modelMetadata = null) {
    var finModel = new ModelImpl<NormalUvVertexImpl>(
        (index, position) => new NormalUvVertexImpl(index, position)) {
        FileBundle = modelMetadata?.fileBundle!,
        Files = modelMetadata?.files!
    };
    var finRootBone = CreateAdjustedRootBone(finModel);
    return (finModel, finRootBone);
  }

  public static IBone CreateAdjustedRootBone(IModel finModel) {
    var finSkeleton = finModel.Skeleton;
    var bone = finSkeleton.Root.AddRoot(0, 0, 0);
    bone.LocalTransform.SetRotationDegrees(-90, 180, 0);
    bone.LocalTransform.SetScale(-1, 1, 1);
    return bone;
  }

  public static void AddToModel(D3d d3d,
                                IModel<ISkin<NormalUvVertexImpl>> finModel,
                                IReadOnlyBone bone,
                                out IMesh finMesh,
                                IMaterial? material = null) {
    var finSkin = finModel.Skin;
    finMesh = finSkin.AddMesh();

    var boneWeights = finSkin.GetOrCreateBoneWeights(
        VertexSpace.RELATIVE_TO_BONE,
        bone);

    D3dPrimitiveType d3dPrimitiveType = default;
    var finVertices = new LinkedList<IVertex>();
    foreach (var d3dCommand in d3d.Commands) {
      switch (d3dCommand.CommandType) {
        case D3dCommandType.BEGIN: {
          d3dPrimitiveType = (D3dPrimitiveType) d3dCommand.Parameters[0];
          break;
        }
        case D3dCommandType.END: {
          switch (d3dPrimitiveType) {
            case D3dPrimitiveType.TRIANGLE_LIST: {
              finMesh.AddTriangles(finVertices.ToArray()).SetMaterial(material);
              break;
            }
            default: throw new NotImplementedException();
          }

          finVertices.Clear();
          break;
        }
        case D3dCommandType.VERTEX_NORMAL_TEXTURE: {
          var d3dParams = d3dCommand.Parameters.AsSpan();

          var finVertex = finSkin.AddVertex(new Vector3(d3dParams.Slice(0, 3)));
          finVertex.SetLocalNormal(-new Vector3(d3dParams.Slice(3, 3)));
          finVertex.SetUv(new Vector2(d3dParams.Slice(6, 2)));
          finVertex.SetBoneWeights(boneWeights);

          finVertices.AddLast(finVertex);
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }
}