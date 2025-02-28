using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.animation.keyframes;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.math.transform;
using fin.model.impl;
using fin.model.util;
using fin.util.sets;

using SharpGLTF.Memory;
using SharpGLTF.Schema2;

using GltfPrimitiveType = SharpGLTF.Schema2.PrimitiveType;

namespace fin.model.io.importers.gltf;

public record GltfModelFileBundle(IReadOnlyTreeFile GltfFile)
    : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.GltfFile;
}

public class GltfModelImporter : IModelImporter<GltfModelFileBundle> {
  public IModel Import(GltfModelFileBundle modelFileBundle) {
    var gltfFile = modelFileBundle.GltfFile;

    var files = gltfFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = files
    };

    var gltf = ModelRoot.Load(gltfFile.FullPath);

    // Adds materials
    var lazyFinImages = new LazyDictionary<MemoryImage, IImage>(
        gltfImage => {
          if (gltfImage.SourcePath != null) {
            return FinImage.FromFile(new FinFile(gltfImage.SourcePath));
          }

          return FinImage.FromStream(
              new MemoryStream(gltfImage.Content.ToArray()));
        });
    var lazyFinTextures = new LazyDictionary<Texture?, ITexture?>(
        gltfTexture => {
          if (gltfTexture == null) {
            return null;
          }

          var finImage = lazyFinImages[gltfTexture.PrimaryImage.Content];

          var finTexture = finModel.MaterialManager.CreateTexture(finImage);
          finTexture.Name = gltfTexture.Name;

          var gltfSampler = gltfTexture.Sampler;
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

          return finTexture;
        });
    var lazyFinMaterials = new LazyDictionary<Material, IMaterial>(
        gltfMaterial => {
          // TODO: Handle all properties within gltfMaterial

          var finMaterial = finModel.MaterialManager.AddStandardMaterial();
          finMaterial.Name = gltfMaterial.Name;

          finMaterial.DiffuseTexture
              = lazyFinTextures[gltfMaterial.GetDiffuseTexture()];
          finMaterial.NormalTexture
              = lazyFinTextures[gltfMaterial.FindChannel("Normal")?.Texture];
          finMaterial.EmissiveTexture
              = lazyFinTextures[gltfMaterial.FindChannel("Emissive")?.Texture];
          finMaterial.AmbientOcclusionTexture
              = lazyFinTextures[gltfMaterial.FindChannel("Occlusion")?.Texture];

          return finMaterial;
        });

    // Adds rig
    var finSkeleton = finModel.Skeleton;
    var finBoneByNode = new Dictionary<Node, IReadOnlyBone>();

    var logicalIndexToJointIndex = new Dictionary<int, int>();
    var finBoneByJointIndex = new Dictionary<int, IReadOnlyBone>();

    foreach (var gltfSkin in gltf.LogicalSkins) {
      for (var i = 0; i < gltfSkin.Joints.Count; ++i) {
        logicalIndexToJointIndex[gltfSkin.Joints[i].LogicalIndex] = i;
      }

      var nodeAndBoneQueue
          = new FinTuple2Queue<Node, IBone?>((gltfSkin.Joints[0], null));
      while (nodeAndBoneQueue.TryDequeue(out var gltfNode,
                                         out var parentFinBone)) {
        var gltfTransform = gltfNode.LocalTransform;

        var finBone = (parentFinBone ?? finSkeleton.Root).AddChild(
            gltfTransform.Translation);
        finBone.LocalTransform.SetRotation(gltfTransform.Rotation);
        finBone.LocalTransform.SetScale(gltfTransform.Scale);

        finBoneByNode[gltfNode] = finBone;

        var name = gltfNode.Name;
        finBone.Name = name;

        finBoneByJointIndex[logicalIndexToJointIndex[gltfNode.LogicalIndex]]
            = finBone;

        nodeAndBoneQueue.Enqueue(
            gltfNode.VisualChildren.Select(childNode => (childNode, finBone)));
      }
    }

    // Adds animations
    foreach (var gltfAnimation in gltf.LogicalAnimations) {
      var finAnimation = finModel.AnimationManager.AddAnimation();
      finAnimation.Name = gltfAnimation.Name;

      var frameRate = finAnimation.FrameRate = 30;
      finAnimation.FrameCount = (int) (gltfAnimation.Duration * frameRate);

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

    foreach (var gltfMesh in gltf.LogicalMeshes) {
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

        var joints0Accessor = gltfPrimitive.GetVertexAccessor("JOINTS_0")
                                           ?.AsVector4Array();
        var weights0Accessor = gltfPrimitive.GetVertexAccessor("WEIGHTS_0")
                                            ?.AsVector4Array();

        var finVertices
            = indexAccessor
              .Select((index) => {
                var i = (int) index;

                var finVertex = finSkin.AddVertex(positionAccessor[i]);

                if (normalAccessor != null) {
                  finVertex.SetLocalNormal(normalAccessor[i]);
                }

                if (texCoord0Accessor != null) {
                  finVertex.SetUv(0, texCoord0Accessor[i]);
                }

                if (joints0Accessor != null &&
                    weights0Accessor != null) {
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
                          VertexSpace.RELATIVE_TO_BONE,
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
                    .SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
      }
    }

    return finModel;
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
                           .Select(
                               tuple => (frameRate * tuple.Key, tuple.Value))
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