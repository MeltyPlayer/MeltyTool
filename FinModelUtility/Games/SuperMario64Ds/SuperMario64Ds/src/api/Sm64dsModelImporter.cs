﻿using System.Numerics;

using fin.animation.keyframes;
using fin.compression;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.math.floats;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.dictionaries;
using fin.image.util;
using fin.util.sets;

using schema.binary;

using sm64ds.schema.bca;
using sm64ds.schema.bmd;
using sm64ds.schema.gx;

namespace sm64ds.api;

public class Sm64dsModelImporter : IModelImporter<Sm64dsModelFileBundle> {
  public IModel Import(Sm64dsModelFileBundle fileBundle) {
    var bmdFile = fileBundle.BmdFile;

    var files = bmdFile.AsFileSet();
    var model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    using var bmdRawBr = bmdFile.OpenReadAsBinary();
    var bmdBr
        = new SchemaBinaryReader(new Lz77Decompressor().Decompress(bmdRawBr));
    var bmd = bmdBr.ReadNew<Bmd>();

    // Set up bones
    var finBones = new IReadOnlyBone[bmd.Bones.Length];
    var sm64Bones = bmd.Bones.OrderBy(b => b.Id).ToArray();
    {
      var rootBones = new List<Bone>();
      var boneToChildMap = new SetDictionary<Bone, Bone>();
      var nextSiblingMap = new Dictionary<Bone, Bone>();
      foreach (var bone in sm64Bones) {
        var offsetToParentBone = bone.OffsetToParentBone;
        if (offsetToParentBone != 0) {
          boneToChildMap.Add(sm64Bones[bone.Id + offsetToParentBone], bone);
        } else {
          rootBones.Add(bone);
        }

        var offsetToNextSibling = bone.OffsetToNextSiblingBone;
        if (offsetToNextSibling != 0) {
          nextSiblingMap[bone] = sm64Bones[bone.Id + offsetToNextSibling];
        }
      }

      var previousSiblingMap = nextSiblingMap.SwapKeysAndValues();

      var boneQueue = new FinTuple2Queue<Bone, IBone>(
          rootBones.Select(b => (b, model.Skeleton.Root)));
      while (boneQueue.TryDequeue(out var bone, out var parentFinBone)) {
        var finBone = parentFinBone.AddChild(bone.Translation);
        finBone.Name = bone.Name;

        finBones[bone.Id] = finBone;

        var localTransform = finBone.LocalTransform;
        localTransform.SetRotationDegrees(bone.Rotation);
        localTransform.SetScale(bone.Scale);

        // Maybe this is for the mesh(es) instead?
        /*if (bone.Billboard) {
          var rotateYaw =
              Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0);
          finBone.AlwaysFaceTowardsCamera(rotateYaw);
        }*/

        if (boneToChildMap.TryGetSet(bone, out var unorderedChildren)) {
          var firstChild
              = unorderedChildren!.Single(b => !previousSiblingMap.ContainsKey(
                                              b));
          boneQueue.Enqueue(nextSiblingMap.Chain(firstChild)
                                          .Select(b => (b, finBone)));
        }
      }
    }

    // Set up materials
    var finMaterialManager = model.MaterialManager;
    var lazyImageDictionary
        = new
            LazyDictionary<(Texture texture, Palette? palette),
                IImage>(textureAndPalette => {
              var (sm64Texture, sm64Palette) = textureAndPalette;
              return ImageReader.ReadImage(sm64Texture, sm64Palette);
            });
    var lazyMaterialDictionary
        = new LazyDictionary<(Material, bool hasNormals, bool hasVertexColor),
            (IReadOnlyMaterial?, IReadOnlyTexture?)>(tuple => {
          var (sm64Material, hasNormals, hasVertexColor) = tuple;

          var textureId = sm64Material.TextureId;
          var paletteId = sm64Material.TexturePaletteId;

          ITexture? finTexture = null;
          if (textureId != -1) {
            var sm64Texture = bmd.Textures[textureId];
            var sm64Palette
                = paletteId != -1 ? bmd.Palettes[paletteId] : null;

            var finImage = lazyImageDictionary[(sm64Texture, sm64Palette)];

            finTexture = finMaterialManager.CreateTexture(finImage);
            finTexture.Name = sm64Texture.Name;

            var sm64TextureParams
                = TextureParamsUtil.GetParams(sm64Material, sm64Texture);

            finTexture.TextureTransform
                      .SetTranslation2d(sm64TextureParams.Translation)
                      .SetRotationRadians2d(sm64TextureParams.Rotation)
                      .SetScale2d(sm64TextureParams.Scale);

            finTexture.WrapModeU = sm64TextureParams.WrapModeS;
            finTexture.WrapModeV = sm64TextureParams.WrapModeT;

            finTexture.MinFilter = TextureMinFilter.NEAR;
            finTexture.MagFilter = TextureMagFilter.NEAR;
          }

          return (CreateMaterial_(sm64Material,
                                  hasVertexColor,
                                  hasNormals,
                                  finMaterialManager,
                                  finTexture), finTexture);
        });

    // Set up mesh
    var lazyOpcodeMap
        = new LazyDictionary<DisplayList, IOpcode[]>(sm64DisplayList => {
          using var opcodeBr
              = new SchemaBinaryReader(sm64DisplayList.Data.OpcodeBytes);
          return OpcodeReader.ReadOpcodes(opcodeBr);
        });

    var scaleFactor = 1 << bmd.ScaleFactor;
    var finSkin = model.Skin;
    foreach (var sm64Bone in sm64Bones) {
      var materialAndDisplayListIds
          = sm64Bone.MaterialIds.Zip(sm64Bone.DisplayListIds).ToArray();
      if (materialAndDisplayListIds.Length == 0) {
        continue;
      }

      var finMesh = finSkin.AddMesh();
      foreach (var (materialId, displayListId) in materialAndDisplayListIds) {
        var sm64Material = bmd.Materials[materialId];
        var displayList = bmd.DisplayLists[displayListId];

        var finBoneByTransformId
            = displayList.Data.TransformIds
                         .Select(transformId
                                     => bmd.TransformToBoneMap[transformId])
                         .Select(boneId => finBones[boneId])
                         .ToArray();

        var opcodes = lazyOpcodeMap[displayList];
        var hasNormals = opcodes.Any(o => o is NormalOpcode);

        var alpha = sm64Material.Alpha;
        var hasVertexColor
            = opcodes.Any(o => o is ColorOpcode) || !alpha.IsRoughly1();

        var (finMaterial, finTexture)
            = lazyMaterialDictionary[(sm64Material, hasNormals,
                                      hasVertexColor)];
        var uvMultiplier = finTexture != null
            ? new Vector2(1f / finTexture.Image.Width,
                          1f / finTexture.Image.Height)
            : Vector2.One;

        PolygonType polygonType = default;
        Vector3 position = default;
        Vector4? color = hasVertexColor ? new Vector4(1, 1, 1, alpha) : null;
        Vector3? normal = null;
        Vector2 uv = default;
        IBoneWeights boneWeights
            = finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                             finBoneByTransformId[0]);

        var finVertices = new LinkedList<IVertex>();

        foreach (var opcode in opcodes) {
          switch (opcode) {
            case TexCoordOpcode texCoordOpcode: {
              uv = texCoordOpcode.TexCoord * uvMultiplier;
              break;
            }
            case ColorOpcode colorOpcode: {
              color = new Vector4(colorOpcode.Color, alpha);
              break;
            }
            case NormalOpcode normalOpcode: {
              normal = normalOpcode.Normal;
              break;
            }
            case IVertexOpcode vertexOpcode: {
              position.X = vertexOpcode.X ?? position.X;
              position.Y = vertexOpcode.Y ?? position.Y;
              position.Z = vertexOpcode.Z ?? position.Z;

              var finVertex = finSkin.AddVertex(position * scaleFactor);
              if (color != null) {
                finVertex.SetColor(color.Value);
              }

              finVertex.SetLocalNormal(normal);
              finVertex.SetUv(uv);
              finVertex.SetBoneWeights(boneWeights);

              finVertices.AddLast(finVertex);
              break;
            }
            case Vertex0x28Opcode vertex0x28Opcode: {
              position += vertex0x28Opcode.DeltaPosition;

              var finVertex = finSkin.AddVertex(position * scaleFactor);
              if (color != null) {
                finVertex.SetColor(color.Value);
              }

              finVertex.SetLocalNormal(normal);
              finVertex.SetUv(uv);
              finVertex.SetBoneWeights(boneWeights);

              finVertices.AddLast(finVertex);
              break;
            }
            case MatrixRestoreOpcode matrixRestoreOpcode: {
              var finBone
                  = finBoneByTransformId[matrixRestoreOpcode.TransformId];
              boneWeights = finSkin.GetOrCreateBoneWeights(
                  VertexSpace.RELATIVE_TO_BONE,
                  finBone);
              break;
            }
            case BeginVertexListOpcode beginVertexListOpcode: {
              polygonType = beginVertexListOpcode.PolygonType;
              break;
            }
            case EndVertexListOpcode: {
              var finPrimitive = polygonType switch {
                  PolygonType.TRIANGLES => finMesh.AddTriangles(
                      finVertices.ToArray()),
                  PolygonType.QUADS => finMesh.AddQuads(finVertices.ToArray()),
                  PolygonType.TRIANGLE_STRIP => finMesh.AddTriangleStrip(
                      finVertices.ToArray()),
                  PolygonType.QUAD_STRIP => finMesh.AddQuadStrip(
                      finVertices.ToArray()),
                  _ => throw new ArgumentOutOfRangeException()
              };
              finVertices.Clear();

              finPrimitive.SetMaterial(finMaterial);
              finPrimitive.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);

              break;
            }
            case NoopOpcode or UnhandledOpcode: {
              break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(opcode));
          }
        }
      }
    }

    // Set up animations
    if (fileBundle.BcaFiles != null) {
      foreach (var bcaFile in fileBundle.BcaFiles) {
        using var bcaRawBr = bcaFile.OpenReadAsBinary();
        var bcaBr = new SchemaBinaryReader(
            new Lz77Decompressor().Decompress(bcaRawBr));
        var bca = bcaBr.ReadNew<Bca>();

        var finAnimation = model.AnimationManager.AddAnimation();
        finAnimation.Name = bcaFile.NameWithoutExtension.ToString();

        finAnimation.FrameRate = 30;
        finAnimation.FrameCount = bca.NumFrames;
        finAnimation.UseLoopingInterpolation = bca.Looped;

        for (var i = 0; i < bca.BoneAnimationData.Length; ++i) {
          var boneAnimationData = bca.BoneAnimationData[i];

          var boneTracks = finAnimation.GetOrCreateBoneTracks(finBones[i]);

          (int, float)[][] translationAxes = [
              boneAnimationData.TranslationXValues,
              boneAnimationData.TranslationYValues,
              boneAnimationData.TranslationZValues
          ];
          var translations = boneTracks.UseSeparateTranslationKeyframes();
          for (var a = 0; a < translationAxes.Length; ++a) {
            var translationAxis = translationAxes[a];
            foreach (var (f, value) in translationAxis) {
              translations.SetKeyframe(a, f, value);
            }
          }

          (int, float)[][] rotationAxes = [
              boneAnimationData.RotationXValues,
              boneAnimationData.RotationYValues,
              boneAnimationData.RotationZValues
          ];
          var rotations = boneTracks.UseSeparateEulerRadiansKeyframes();
          for (var a = 0; a < rotationAxes.Length; ++a) {
            var rotationAxis = rotationAxes[a];
            foreach (var (f, value) in rotationAxis) {
              rotations.SetKeyframe(a, f, value);
            }
          }

          (int, float)[][] scaleAxes = [
              boneAnimationData.ScaleXValues,
              boneAnimationData.ScaleYValues,
              boneAnimationData.ScaleZValues
          ];
          var scales = boneTracks.UseSeparateScaleKeyframes();
          for (var a = 0; a < scaleAxes.Length; ++a) {
            var scaleAxis = scaleAxes[a];
            foreach (var (f, value) in scaleAxis) {
              scales.SetKeyframe(a, f, value);
            }
          }
        }
      }
    }

    return model;
  }

  private static IMaterial CreateMaterial_(Material sm64Material,
                                           bool hasVertexColor,
                                           bool hasNormals,
                                           IMaterialManager materialManager,
                                           IReadOnlyTexture? texture) {
    var finMaterial = materialManager.AddFixedFunctionMaterial();
    finMaterial.Name = texture?.Name;
    finMaterial.CullingMode = sm64Material.CullMode;

    var equations = finMaterial.Equations;

    var diffuseColorAlpha = finMaterial.GenerateDiffuse(
        (equations.CreateColorConstant(sm64Material.DiffuseColor.Rgbf),
         equations.CreateScalarConstant(sm64Material.DiffuseColor.Af)),
        texture,
        (hasVertexColor, hasVertexColor));

    var outputColorAlpha
        = hasNormals
            ? equations.GenerateLighting(
                diffuseColorAlpha,
                equations.CreateColorConstant(sm64Material.AmbientColor.Rgbf),
                equations.CreateColorConstant(sm64Material.SpecularColor.Rgbf),
                equations.CreateColorConstant(sm64Material.EmissionColor.Rgbf))
            : diffuseColorAlpha;

    equations.SetOutputColorAlpha(outputColorAlpha);

    finMaterial.TransparencyType = hasVertexColor
        ? TransparencyType.TRANSPARENT
        : finMaterial.Textures.FirstOrDefault()?.TransparencyType ??
          TransparencyType.OPAQUE;
    finMaterial.SetDefaultAlphaCompare();


    return finMaterial;
  }
}