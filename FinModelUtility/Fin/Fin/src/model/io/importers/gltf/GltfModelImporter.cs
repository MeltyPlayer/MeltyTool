using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

using fin.animation.keyframes;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.math.matrix.four;
using fin.math.transform;
using fin.model.impl;
using fin.model.util;
using fin.util.sets;

using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;

using GltfPrimitiveType = SharpGLTF.Schema2.PrimitiveType;
using Material = SharpGLTF.Schema2.Material;
using Node = SharpGLTF.Schema2.Node;
using TextureTransform = SharpGLTF.Schema2.TextureTransform;
using TextureWrapMode = SharpGLTF.Schema2.TextureWrapMode;

namespace fin.model.io.importers.gltf;

public record GltfModelFileBundle(IReadOnlyTreeFile GltfFile)
    : IModelFileBundle<GltfModelFileBundle, GltfModelImporter> {
  public IReadOnlyTreeFile MainFile => this.GltfFile;
  public Action<IModel>? AdditionalProcessing { get; init; }
}

public sealed class GltfModelImporter
    : IModelImporter<GltfModelImporter, GltfModelFileBundle> {
  public IModel Import(GltfModelFileBundle modelFileBundle) {
    var gltfFile = modelFileBundle.GltfFile;

    var files = gltfFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = files
    };

    var gltf = ModelRoot.Load(gltfFile.FullPath,
                              new ReadSettings
                                  { Validation = ValidationMode.Skip });

    // Adds materials
    var lazyFinImages = new LazyDictionary<MemoryImage, IImage>(gltfImage => {
      if (gltfImage.SourcePath != null) {
        return FinImage.FromFile(new FinFile(gltfImage.SourcePath));
      }

      return FinImage.FromStream(
          new MemoryStream(gltfImage.Content.ToArray()));
    });
    var lazyFinTextures
        = new LazyDictionary<(Texture?, TextureTransform?), ITexture?>(tuple
            => {
          var (gltfTexture, gltfTextureTransform) = tuple;
          if (gltfTexture == null) {
            return null;
          }

          var finImage = lazyFinImages[gltfTexture.PrimaryImage.Content];

          var finTexture = finModel.MaterialManager.CreateTexture(finImage);
          finTexture.Name = gltfTexture.Name;

          var gltfSampler = gltfTexture.Sampler;
          if (gltfSampler != null) {
            finTexture.WrapModeU = ConvertWrapMode_(gltfSampler.WrapS);
            finTexture.WrapModeV = ConvertWrapMode_(gltfSampler.WrapT);

            finTexture.MinFilter = gltfSampler.MinFilter switch {
                TextureMipMapFilter.NEAREST => TextureMinFilter.NEAR,
                TextureMipMapFilter.LINEAR  => TextureMinFilter.LINEAR,
                TextureMipMapFilter.NEAREST_MIPMAP_NEAREST => TextureMinFilter
                    .NEAR_MIPMAP_NEAR,
                TextureMipMapFilter.LINEAR_MIPMAP_NEAREST => TextureMinFilter
                    .LINEAR_MIPMAP_NEAR,
                TextureMipMapFilter.NEAREST_MIPMAP_LINEAR => TextureMinFilter
                    .NEAR_MIPMAP_LINEAR,
                TextureMipMapFilter.LINEAR_MIPMAP_LINEAR => TextureMinFilter
                    .LINEAR_MIPMAP_LINEAR,
                TextureMipMapFilter.DEFAULT => TextureMinFilter
                    .LINEAR_MIPMAP_LINEAR,
                _ => throw new ArgumentOutOfRangeException()
            };
            finTexture.MagFilter = gltfSampler.MagFilter switch {
                TextureInterpolationFilter.NEAREST => TextureMagFilter.NEAR,
                TextureInterpolationFilter.LINEAR => TextureMagFilter.LINEAR,
                TextureInterpolationFilter.DEFAULT => TextureMagFilter.LINEAR,
                _ => throw new ArgumentOutOfRangeException()
            };
          }

          if (gltfTextureTransform != null) {
            var finTextureTransform = finTexture.TextureTransform;
            finTextureTransform.SetTranslation2d(gltfTextureTransform.Offset);
            finTextureTransform.SetRotationRadians2d(gltfTextureTransform.Rotation);
            finTextureTransform.SetScale2d(gltfTextureTransform.Scale);
          }

          return finTexture;
        });
    var lazyFinMaterials = new LazyDictionary<Material, IMaterial>(gltfMaterial
        => {
      // TODO: Handle all properties within gltfMaterial

      if (gltfMaterial is null) {
        return finModel.MaterialManager.AddColorMaterial(Color.White);
      }

      var finMaterial = finModel.MaterialManager.AddStandardMaterial();
      finMaterial.Name = gltfMaterial.Name;

      finMaterial.DiffuseTexture
          = lazyFinTextures[(gltfMaterial.GetDiffuseTexture(), gltfMaterial.GetDiffuseTextureTransform())];
      var diffuseColor
          = (gltfMaterial.FindChannel("BaseColor")?.Color ?? Vector4.One) *
            gltfMaterial.GetDiffuseColor(Vector4.One) * 255;
      finMaterial.DiffuseColor = Color.FromArgb(
          (byte) diffuseColor.W,
          (byte) diffuseColor.X,
          (byte) diffuseColor.Y,
          (byte) diffuseColor.Z);
      finMaterial.NormalTexture
          = lazyFinTextures[(gltfMaterial.FindChannel("Normal")?.Texture, null)];
      finMaterial.EmissiveTexture
          = lazyFinTextures[(gltfMaterial.FindChannel("Emissive")?.Texture, null)];
      finMaterial.AmbientOcclusionTexture
          = lazyFinTextures[(gltfMaterial.FindChannel("Occlusion")?.Texture, null)];

      finMaterial.IgnoreLights = gltfMaterial.Unlit;
      finMaterial.CullingMode = gltfMaterial.DoubleSided
          ? CullingMode.SHOW_BOTH
          : CullingMode.SHOW_FRONT_ONLY;

      var metallicRoughness = gltfMaterial.FindChannel("MetallicRoughness");

      var specularGlossiness = gltfMaterial.FindChannel("SpecularGlossiness");
      if (specularGlossiness != null) {
        if (specularGlossiness
            ?.Parameters.FirstOrDefault(p => p.Name ==
                                             "SpecularFactor")
            ?.Value is Vector3 specularColor) {
          // TODO: Pass this color into the material
        }

        if (specularGlossiness
            ?.Parameters.FirstOrDefault(p => p.Name ==
                                             "GlossinessFactor")
            ?.Value is float glossiness) {
          finMaterial.Shininess = 100 * glossiness;
        }
      }

      return finMaterial;
    });

    // Adds rig
    var finSkeleton = finModel.Skeleton;
    var finBoneByNode = new Dictionary<Node, IBone>();

    var logicalIndexToJointIndex = new Dictionary<int, int>();
    var finBoneByJointIndex = new Dictionary<int, IReadOnlyBone>();
    var finBoneWeightsByNode = new Dictionary<Node, IReadOnlyBoneWeights>();

    foreach (var gltfSkin in gltf.LogicalSkins) {
      // TODO: Better way to get this?
      var gltfRootNode = gltfSkin.Joints[0].VisualParent;

      var root
          = finSkeleton.Root.AddChild(gltfSkin.Skeleton?.LocalMatrix ??
                                      Matrix4x4.Identity);

      for (var i = 0; i < gltfSkin.Joints.Count; ++i) {
        logicalIndexToJointIndex[gltfSkin.Joints[i].LogicalIndex] = i;
      }

      var nodeAndBoneQueue = new FinTuple2Queue<Node, IBone>(
          gltfSkin.Joints.Where(j => j.VisualParent == gltfRootNode)
                  .Select(j => (j, root)));
      while (nodeAndBoneQueue.TryDequeue(out var gltfNode,
                                         out var parentFinBone)) {
        var finBone
            = AddChildBone_(parentFinBone, gltfNode);

        finBoneByNode[gltfNode] = finBone;
        finBoneWeightsByNode[gltfNode]
            = finModel.Skin.GetOrCreateBoneWeights(
                VertexSpace.RELATIVE_TO_WORLD,
                finBone);

        if (logicalIndexToJointIndex.TryGetValue(gltfNode.LogicalIndex,
                                                 out var jointIndex)) {
          finBoneByJointIndex[jointIndex] = finBone;
        }

        nodeAndBoneQueue.Enqueue(
            gltfNode.VisualChildren.Select(childNode => (childNode, finBone)));
      }
    }

    // Adds animations
    foreach (var gltfAnimation in gltf.LogicalAnimations) {
      var finAnimation = finModel.AnimationManager.AddAnimation();
      finAnimation.Name = gltfAnimation.Name;

      var frameRate = finAnimation.FrameRate = 30;
      finAnimation.FrameCount
          = Math.Max(1, (int) (gltfAnimation.Duration * frameRate));

      foreach (var gltfAnimationChannel in gltfAnimation.Channels) {
        if (finBoneByNode.TryGetValue(gltfAnimationChannel.TargetNode,
                                      out var finBone)) {
          var finBoneTracks = finAnimation.GetOrCreateBoneTracks(finBone);
          AddGltfAnimationChannelToFinBoneTracks_(gltfAnimationChannel,
                                                  finBoneTracks,
                                                  frameRate);
        }
      }
    }

    // Adds skin
    var finSkin = finModel.Skin;

    var logicalNodesByParent = new SetDictionary<Node?, Node>();
    foreach (var gltfNode in gltf.LogicalNodes) {
      logicalNodesByParent.Add(gltfNode.VisualParent, gltfNode);
    }

    var nodeQueue = new FinTuple3Queue<Node, IReadOnlyBoneWeights?, Matrix4x4>(
        logicalNodesByParent[null]
            .Select(n => (n, finBoneWeightsByNode.GetValueOrDefault(n),
                          Matrix4x4.Identity)));
    while (nodeQueue.TryDequeue(out var gltfNode,
                                out var finBoneWeights,
                                out var matrix)) {
      matrix = gltfNode.LocalMatrix * matrix;

      if (logicalNodesByParent.TryGetSet(gltfNode, out var children)) {
        nodeQueue.Enqueue(
            children!.Select(n => (n,
                                   finBoneWeightsByNode.GetValueOrDefault(
                                       n,
                                       finBoneWeights), matrix)));
      }

      var gltfMesh = gltfNode.Mesh;
      if (gltfMesh == null) {
        continue;
      }

      matrix.AssertDecompose(out _,
                             out _,
                             out var scale);

      var flippedInsideOut = scale.X < 0 || scale.Y < 0 || scale.Z < 0;

      var finMesh = finSkin.AddMesh();
      finMesh.Name = gltfMesh.Name;

      foreach (var gltfPrimitive in gltfMesh.Primitives) {
        var finMaterial = lazyFinMaterials[gltfPrimitive.Material];

        var indexAccessor = gltfPrimitive.GetIndexAccessor().AsIndicesArray();
        var positionAccessor = gltfPrimitive.GetVertexAccessor("POSITION")
                                            .AsVector3Array();

        var normalAccessor
            = gltfPrimitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
        var texCoord0Accessor = gltfPrimitive.GetVertexAccessor("TEXCOORD_0")
                                             ?
                                             .AsVector2Array();
        var color0Accessor
            = gltfPrimitive.GetVertexAccessor("COLOR_0 ")?.AsVector4Array();

        var joints0Accessor = gltfPrimitive.GetVertexAccessor("JOINTS_0")
                                           ?.AsVector4Array();
        var weights0Accessor = gltfPrimitive.GetVertexAccessor("WEIGHTS_0")
                                            ?.AsVector4Array();

        var finVertices
            = indexAccessor
              .Select((index) => {
                var i = (int) index;

                var gltfPosition = positionAccessor[i];
                var finVertex
                    = finSkin.AddVertex(
                        Vector3.Transform(gltfPosition, matrix));

                if (normalAccessor != null) {
                  var localNormal = normalAccessor[i];
                  if (flippedInsideOut) {
                    localNormal = -localNormal;
                  }

                  finVertex.SetLocalNormal(
                      Vector3.TransformNormal(localNormal, matrix));
                }

                if (texCoord0Accessor != null) {
                  finVertex.SetUv(0, texCoord0Accessor[i]);
                }

                if (color0Accessor != null) {
                  finVertex.SetColor(color0Accessor[i]);
                }

                if (joints0Accessor == null || weights0Accessor == null) {
                  if (finBoneWeights != null) {
                    finVertex.SetBoneWeights(finBoneWeights);
                  }
                } else {
                  var joints0 = joints0Accessor[i];
                  var weights0 = weights0Accessor[i];

                  var boneWeights = new LinkedList<IReadOnlyBoneWeight>();
                  for (var b = 0; b < 4; ++b) {
                    var weight = weights0[b];
                    if (weight == 0) {
                      break;
                    }

                    boneWeights.AddLast(
                        new BoneWeight(finBoneByJointIndex[(int) joints0[b]],
                                       null,
                                       weight));
                  }

                  finVertex.SetBoneWeights(
                      finSkin.GetOrCreateBoneWeights(
                          VertexSpace.RELATIVE_TO_WORLD,
                          boneWeights.ToArray()
                      ));
                }

                return finVertex;
              })
              .ToArray();

        var finPrimitive = gltfPrimitive.DrawPrimitiveType switch {
            GltfPrimitiveType.POINTS     => finMesh.AddPoints(finVertices),
            GltfPrimitiveType.LINES      => finMesh.AddLines(finVertices),
            GltfPrimitiveType.LINE_STRIP => finMesh.AddLineStrip(finVertices),
            GltfPrimitiveType.TRIANGLES  => finMesh.AddTriangles(finVertices),
            GltfPrimitiveType.TRIANGLE_STRIP => finMesh.AddTriangleStrip(
                finVertices),
            GltfPrimitiveType.TRIANGLE_FAN => finMesh.AddTriangleFan(
                finVertices),
            _ => throw new ArgumentOutOfRangeException()
        };

        finPrimitive.SetMaterial(finMaterial)
                    .SetVertexOrder(flippedInsideOut
                                        ? VertexOrder.CLOCKWISE
                                        : VertexOrder.COUNTER_CLOCKWISE);
      }
    }

    modelFileBundle.AdditionalProcessing?.Invoke(finModel);

    return finModel;
  }

  private static IBone AddChildBone_(IBone parentFinBone, Node gltfNode) {
    var gltfTransform = gltfNode.LocalTransform;

    var finBone = parentFinBone.AddChild(gltfTransform.Translation);
    finBone.Transform.SetRotation(gltfTransform.Rotation);
    finBone.Transform.SetScale(gltfTransform.Scale);

    finBone.Name = gltfNode.Name;

    return finBone;
  }

  private static WrapMode ConvertWrapMode_(TextureWrapMode gltfWrapMode)
    => gltfWrapMode switch {
        TextureWrapMode.REPEAT          => WrapMode.REPEAT,
        TextureWrapMode.CLAMP_TO_EDGE   => WrapMode.CLAMP,
        TextureWrapMode.MIRRORED_REPEAT => WrapMode.MIRROR_REPEAT,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gltfWrapMode),
            gltfWrapMode,
            null)
    };

  private static void AddGltfAnimationChannelToFinBoneTracks_(
      AnimationChannel gltfAnimationChannel,
      IBoneTracks finBoneTracks,
      float frameRate) {
    var gltfTranslations = gltfAnimationChannel.GetTranslationSampler();
    if (gltfTranslations != null) {
      switch (gltfTranslations.InterpolationMode) {
        case AnimationInterpolationMode.LINEAR: {
          var finTranslations = finBoneTracks.UseCombinedTranslationKeyframes();
          foreach (var gltfKey in gltfTranslations.GetLinearKeys()) {
            finTranslations.SetKeyframe(gltfKey.Key * frameRate, gltfKey.Value);
          }

          break;
        }
        case AnimationInterpolationMode.STEP: {
          var finTranslations = finBoneTracks.UseCombinedTranslationKeyframes();
          finTranslations.SetAllStepKeyframes(
              gltfTranslations.GetLinearKeys()
                              .Select(tuple => (
                                          frameRate * tuple.Key, tuple.Value))
                              .ToArray());

          break;
        }
        case AnimationInterpolationMode.CUBICSPLINE: {
          var finTranslations
              = finBoneTracks.UseSeparateTranslationKeyframesWithTangents();
          foreach (var gltfKey in gltfTranslations.GetCubicKeys()) {
            var frame = gltfKey.Key * frameRate;
            var (tangentIn, value, tangentOut) = gltfKey.Value;

            finTranslations.Axes[0]
                           .SetKeyframe(frame,
                                        value.X,
                                        tangentIn.X,
                                        tangentOut.X);
            finTranslations.Axes[1]
                           .SetKeyframe(frame,
                                        value.Y,
                                        tangentIn.Y,
                                        tangentOut.Y);
            finTranslations.Axes[2]
                           .SetKeyframe(frame,
                                        value.Z,
                                        tangentIn.Z,
                                        tangentOut.Z);
          }

          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }

    var gltfRotations = gltfAnimationChannel.GetRotationSampler();
    if (gltfRotations != null) {
      switch (gltfRotations.InterpolationMode) {
        case AnimationInterpolationMode.LINEAR: {
          var finRotations = finBoneTracks.UseCombinedQuaternionKeyframes();
          foreach (var gltfKey in gltfRotations.GetLinearKeys()) {
            finRotations.SetKeyframe(gltfKey.Key * frameRate, gltfKey.Value);
          }

          break;
        }
        case AnimationInterpolationMode.CUBICSPLINE: {
          var finRotations
              = finBoneTracks.UseSeparateQuaternionKeyframesWithTangents();
          foreach (var gltfKey in gltfRotations.GetCubicKeys()) {
            var frame = gltfKey.Key * frameRate;
            var (tangentIn, value, tangentOut) = gltfKey.Value;

            finRotations.Axes[0]
                        .SetKeyframe(frame,
                                     value.X,
                                     tangentIn.X,
                                     tangentOut.X);
            finRotations.Axes[1]
                        .SetKeyframe(frame,
                                     value.Y,
                                     tangentIn.Y,
                                     tangentOut.Y);
            finRotations.Axes[2]
                        .SetKeyframe(frame,
                                     value.Z,
                                     tangentIn.Z,
                                     tangentOut.Z);
            finRotations.Axes[3]
                        .SetKeyframe(frame,
                                     value.W,
                                     tangentIn.W,
                                     tangentOut.W);
          }

          break;
        }
        case AnimationInterpolationMode.STEP: {
          var finRotations = finBoneTracks.UseCombinedQuaternionKeyframes();
          finRotations.SetAllStepKeyframes(
              gltfRotations.GetLinearKeys()
                           .Select(tuple => (
                                       frameRate * tuple.Key, tuple.Value))
                           .ToArray());

          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }

    var gltfScales = gltfAnimationChannel.GetScaleSampler();
    if (gltfScales != null) {
      switch (gltfScales.InterpolationMode) {
        case AnimationInterpolationMode.LINEAR: {
          var finScales = finBoneTracks.UseCombinedScaleKeyframes();
          foreach (var gltfKey in gltfScales.GetLinearKeys()) {
            finScales.SetKeyframe(gltfKey.Key * frameRate, gltfKey.Value);
          }

          break;
        }
        case AnimationInterpolationMode.STEP: {
          var finScales = finBoneTracks.UseCombinedScaleKeyframes();
          finScales.SetAllStepKeyframes(
              gltfScales.GetLinearKeys()
                        .Select(tuple => (frameRate * tuple.Key, tuple.Value))
                        .ToArray());

          break;
        }
        case AnimationInterpolationMode.CUBICSPLINE: {
          var finScales
              = finBoneTracks.UseSeparateScaleKeyframesWithTangents();
          foreach (var gltfKey in gltfScales.GetCubicKeys()) {
            var frame = gltfKey.Key * frameRate;
            var (tangentIn, value, tangentOut) = gltfKey.Value;

            finScales.Axes[0]
                     .SetKeyframe(frame, value.X, tangentIn.X, tangentOut.X);
            finScales.Axes[1]
                     .SetKeyframe(frame, value.Y, tangentIn.Y, tangentOut.Y);
            finScales.Axes[2]
                     .SetKeyframe(frame, value.Z, tangentIn.Z, tangentOut.Z);
          }

          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }
}