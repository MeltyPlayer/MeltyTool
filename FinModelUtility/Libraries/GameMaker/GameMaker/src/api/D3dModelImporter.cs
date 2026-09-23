using System.Numerics;

using fin.animation.keyframes;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;

using gm.schema.d3d;


namespace gm.api;

public sealed class D3dModelFileBundle
    : IModelFileBundle<D3dModelFileBundle, D3dModelImporter> {
  public required IReadOnlyTreeFile D3dFile { get; init; }

  public IReadOnlyTreeFile? TextureFile { get; init; }

  public (IReadOnlyTreeFile gifFile, float frameRate)? AnimatedTextureFile {
    get;
    init;
  }

  public IReadOnlyTreeFile MainFile => this.D3dFile;
  public bool FlipNormals { get; init; }
  public WrapMode TextureWrapMode { get; init; }
}

public sealed class D3dModelImporter
    : IModelImporter<D3dModelImporter, D3dModelFileBundle> {
  public IModel Import(D3dModelFileBundle modelFileBundle) {
    var d3dFile = modelFileBundle.D3dFile;
    var fileSet = new HashSet<IReadOnlyStandaloneFile>();

    var (finModel, finRootBone)
        = CreateModel((modelFileBundle, fileSet));

    AddToModel(modelFileBundle, finModel, fileSet, finRootBone);

    if (modelFileBundle.FlipNormals) {
      finModel.FlipAllNormals();
    }

    return finModel;
  }

  public static void AddToModel(
      D3dModelFileBundle modelFileBundle,
      IModel<ISkin<Normal1Color1UvVertexImpl>> finModel,
      ISet<IReadOnlyStandaloneFile> fileSet,
      IBone finRootBone,
      bool flipNormals = false) {
    var d3dFile = modelFileBundle.D3dFile;
    var d3d = d3dFile.ReadNewFromText<D3d>();
    fileSet.Add(d3dFile);

    ITextureMaterial? material = null;
    if (modelFileBundle.TextureFile is { } textureFile) {
      (material, var texture) =
          finModel.MaterialManager.AddSimpleTextureMaterialFromFile(
              textureFile);
      texture.WrapModeU = texture.WrapModeV = modelFileBundle.TextureWrapMode;
    } else if (modelFileBundle.AnimatedTextureFile is { } gifFileAndFrameRate) {
      var (gifFile, frameRate) = gifFileAndFrameRate;

      var frameImages = FinImage.FromGifFile(gifFile);
      var name = gifFile.NameWithoutExtension.ToString();

      var finMaterialManager = finModel.MaterialManager;
      var frameTextures
          = frameImages
            .Select((frameImage, i) => {
              var frameTexture = finMaterialManager.CreateTexture(
                  frameImage.RemoveTopLeftBackgroundColor());
              frameTexture.Name =
                  frameImages.Length > 0 ? $"{name}_{i}" : name;
              frameTexture.WrapModeU = frameTexture.WrapModeV =
                  modelFileBundle.TextureWrapMode;
              return frameTexture;
            })
            .ToArray();

      var baseTexture = frameTextures[0];

      material = finMaterialManager.AddTextureMaterial(baseTexture);
      material.Name = "diffuse";

      var animation = finModel.AnimationManager.AddAnimation();
      animation.Name = name;
      animation.FrameCount = frameImages.Length;
      animation.FrameRate = frameRate;

      var textureTracks = animation.AddTextureTracks(baseTexture);
      var flipbookSwapKeyframes
          = textureTracks.UseFlipbookSwapKeyframes(frameTextures.Length);
      for (var f = 0; f < frameTextures.Length; ++f) {
        flipbookSwapKeyframes.Add(
            new Keyframe<IReadOnlyTexture?>(f, frameTextures[f]));
      }
    }

    AddToModel(d3d, finModel, finRootBone, out _, material, flipNormals);
  }

  public static (IModel<ISkin<Normal1Color1UvVertexImpl>>, IBone) CreateModel(
      (IFileBundle fileBundle, IReadOnlySet<IReadOnlyStandaloneFile> files)?
          modelMetadata = null) {
    var finModel =
        new ModelImpl<Normal1Color1UvVertexImpl>((index, position)
                                                     => new
                                                         Normal1Color1UvVertexImpl(
                                                             index,
                                                             position)) {
            FileBundle = modelMetadata?.fileBundle!,
            Files = modelMetadata?.files!
        };
    var finRootBone = CreateAdjustedRootBone(finModel);
    return (finModel, finRootBone);
  }

  public static IBone CreateAdjustedRootBone(IModel finModel) {
    var finSkeleton = finModel.Skeleton;
    var bone = finSkeleton.Root.AddRoot(0, 0, 0);
    bone.Transform.SetRotationDegrees(-90, 180, 0);
    bone.Transform.SetScale(-1, 1, 1);
    return bone;
  }

  public static void AddToModel(
      D3d d3d,
      IModel<ISkin<Normal1Color1UvVertexImpl>> finModel,
      IReadOnlyBone bone,
      out IMesh finMesh,
      IMaterial? material = null,
      bool flipNormals = false) {
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
            case D3dPrimitiveType.TRIANGLE_FAN: {
              finMesh.AddTriangleFan(finVertices.ToArray())
                     .SetMaterial(material);
              break;
            }
            case D3dPrimitiveType.TRIANGLE_LIST: {
              finMesh.AddTriangles(finVertices.ToArray())
                     .SetMaterial(material);
              break;
            }
            case D3dPrimitiveType.TRIANGLE_STRIP: {
              finMesh.AddTriangleStrip(finVertices.ToArray())
                     .SetMaterial(material);
              break;
            }
            default: throw new NotImplementedException();
          }

          finVertices.Clear();
          break;
        }
        case D3dCommandType.VERTEX_NORMAL_TEXTURE: {
          var d3dParams = d3dCommand.Parameters.AsSpan();

          var finVertex = finSkin.AddVertex(new Vector3(d3dParams[..3]));

          var normal = -new Vector3(d3dParams.Slice(3, 3));
          if (flipNormals) {
            normal *= -1;
          }

          finVertex.SetLocalNormal(normal);
          finVertex.SetUv(new Vector2(d3dParams.Slice(6, 2)));
          finVertex.SetBoneWeights(boneWeights);

          finVertices.AddLast(finVertex);
          break;
        }
        case D3dCommandType.VERTEX_TEXTURE: {
          var d3dParams = d3dCommand.Parameters.AsSpan();

          var finVertex = finSkin.AddVertex(new Vector3(d3dParams[..3]));
          finVertex.SetUv(new Vector2(d3dParams.Slice(3, 2)));
          finVertex.SetBoneWeights(boneWeights);

          finVertices.AddLast(finVertex);
          break;
        }
        case D3dCommandType.VERTEX_TEXTURE_COLOR: {
          var d3dParams = d3dCommand.Parameters.AsSpan();

          var finVertex = finSkin.AddVertex(new Vector3(d3dParams[..3]));
          finVertex.SetUv(new Vector2(d3dParams.Slice(3, 2)));
          finVertex.SetColor(new Vector3(d3dParams.Slice(5, 3)) / 255f);
          finVertex.SetBoneWeights(boneWeights);

          finVertices.AddLast(finVertex);
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }
}