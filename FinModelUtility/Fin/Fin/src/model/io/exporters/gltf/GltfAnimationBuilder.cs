using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.data.indexable;

using SharpGLTF.Schema2;

namespace fin.model.io.exporters.gltf;

using GltfNode = Node;

public sealed class GltfAnimationBuilder {
  public void BuildAnimations(
      ModelRoot gltfModel,
      (GltfNode, IReadOnlyBone)[] skinNodesAndBones,
      IReadOnlyDictionary<IReadOnlyMesh, Node> gltfNodeByFinMesh,
      float modelScale,
      IReadOnlyList<IMorphTarget> morphTargets,
      IReadOnlyList<IReadOnlyModelAnimation> animations) {
    foreach (var animation in animations) {
      this.BuildAnimation_(
          gltfModel,
          skinNodesAndBones,
          gltfNodeByFinMesh,
          modelScale,
          morphTargets,
          animation);
    }
  }

  private void BuildAnimation_(
      ModelRoot gltfModel,
      (GltfNode, IReadOnlyBone)[] skinNodesAndBones,
      IReadOnlyDictionary<IReadOnlyMesh, Node> gltfNodeByFinMesh,
      float modelScale,
      IReadOnlyList<IMorphTarget> morphTargets,
      IReadOnlyModelAnimation animation) {
    var hasBoneAnimation
        = animation
          .BoneTracks
          .Any(finBoneTracks
                   => (finBoneTracks.Translations?.HasAnyData ?? false) ||
                      (finBoneTracks.Rotations?.HasAnyData ?? false) ||
                      (finBoneTracks.Scales?.HasAnyData ?? false));
    var hasMorphAnimation = animation.MorphTargetFrames.Count > 0;

    if (!hasBoneAnimation && !hasMorphAnimation) {
      return;
    }

    var gltfAnimation = gltfModel.UseAnimation(animation.Name);

    var fps = animation.FrameRate;

    // Writes translation/rotation/scale for each joint.
    var translationKeyframes = new Dictionary<float, Vector3>();
    var translationTangentKeyframes
        = new Dictionary<float, (Vector3, Vector3, Vector3)>();
    var rotationKeyframes = new Dictionary<float, Quaternion>();
    var rotationTangentKeyframes
        = new Dictionary<float, (Quaternion, Quaternion, Quaternion)>();
    var scaleKeyframes = new Dictionary<float, Vector3>();
    var scaleTangentKeyframes
        = new Dictionary<float, (Vector3, Vector3, Vector3)>();

    var visibilityKeyframes = new Dictionary<float, bool>();

    if (hasMorphAnimation) {
      var allMorphTargets = gltfModel.LogicalMeshes.FirstOrDefault()
                                     ?.Primitives.FirstOrDefault()
                                     ?.MorphTargetsCount ?? 0;
      if (allMorphTargets > 0) {
        var morphTargetIndices = morphTargets
                                 .Select((target, index) => (target, index))
                                 .ToDictionary(pair => pair.target,
                                               pair => pair.index);
        var morphKeyframes = new Dictionary<float, float[]>();
        for (var frame = 0;
             frame < animation.MorphTargetFrames.Count;
             ++frame) {
          var weights = new float[allMorphTargets];
          var morphTarget = animation.MorphTargetFrames[frame];
          if (morphTarget != null &&
              morphTargetIndices.TryGetValue(morphTarget,
                                             out var morphTargetIndex)) {
            weights[morphTargetIndex] = 1;
          }

          morphKeyframes[frame / fps] = weights;
        }

        foreach (var gltfNode in gltfNodeByFinMesh.Values) {
          gltfAnimation.CreateMorphChannel(gltfNode,
                                           morphKeyframes,
                                           allMorphTargets,
                                           true);
        }
      }
    }

    Span<Vector3> translationsOrScales
        = stackalloc Vector3[animation.FrameCount];
    Span<Quaternion> rotations = stackalloc Quaternion[animation.FrameCount];

    foreach (var (node, bone) in skinNodesAndBones) {
      if (!animation.BoneTracks.TryGetValue(bone, out var boneTracks)) {
        continue;
      }

      var translationDefined = boneTracks.Translations?.HasAnyData ?? false;
      if (translationDefined) {
        translationKeyframes.Clear();
        translationTangentKeyframes.Clear();
        if (boneTracks.Translations.TryGetSimpleKeyframes(
                out var keyframes,
                out var tangentKeyframes)) {
          if (tangentKeyframes == null) {
            foreach (var (frame, value) in keyframes) {
              translationKeyframes[frame / fps] = value * modelScale;
            }

            gltfAnimation.CreateTranslationChannel(node, translationKeyframes);
          } else {
            foreach (var (frameAndValue, tangents) in keyframes.Zip(
                         tangentKeyframes)) {
              var (frame, value) = frameAndValue;
              var (tangentIn, tangentOut) = tangents;
              translationTangentKeyframes[frame / fps] = (
                  tangentIn, value * modelScale, tangentOut);
            }

            gltfAnimation.CreateTranslationChannel(
                node,
                translationTangentKeyframes);
          }
        } else {
          boneTracks.Translations.GetAllFrames(translationsOrScales);
          for (var i = 0; i < translationsOrScales.Length; ++i) {
            var time = i / fps;
            translationKeyframes[time] = translationsOrScales[i] * modelScale;
          }

          gltfAnimation.CreateTranslationChannel(node, translationKeyframes);
        }
      }

      var rotationDefined = boneTracks.Rotations?.HasAnyData ?? false;
      if (rotationDefined) {
        rotationKeyframes.Clear();
        rotationTangentKeyframes.Clear();
        if (boneTracks.Rotations.TryGetSimpleKeyframes(
                out var keyframes,
                out var tangentKeyframes)) {
          if (tangentKeyframes == null) {
            foreach (var (frame, value) in keyframes) {
              rotationKeyframes[frame / fps] = value;
            }

            gltfAnimation.CreateRotationChannel(node, rotationKeyframes);
          } else {
            foreach (var (frameAndValue, tangents) in keyframes.Zip(
                         tangentKeyframes)) {
              var (frame, value) = frameAndValue;
              var (tangentIn, tangentOut) = tangents;
              rotationTangentKeyframes[frame / fps]
                  = (tangentIn, value, tangentOut);
            }

            gltfAnimation.CreateRotationChannel(node, rotationTangentKeyframes);
          }
        } else {
          boneTracks.Rotations.GetAllFrames(rotations);
          for (var i = 0; i < rotations.Length; ++i) {
            var time = i / fps;
            rotationKeyframes[time] = rotations[i];
          }

          gltfAnimation.CreateRotationChannel(node, rotationKeyframes);
        }
      }

      var scaleDefined = boneTracks.Scales?.HasAnyData ?? false;
      if (scaleDefined) {
        scaleKeyframes.Clear();
        scaleTangentKeyframes.Clear();
        if (boneTracks.Scales.TryGetSimpleKeyframes(
                out var keyframes,
                out var tangentKeyframes)) {
          if (tangentKeyframes == null) {
            foreach (var (frame, value) in keyframes) {
              scaleKeyframes[frame / fps] = value;
            }

            gltfAnimation.CreateScaleChannel(node, scaleKeyframes);
          } else {
            foreach (var (frameAndValue, tangents) in keyframes.Zip(
                         tangentKeyframes)) {
              var (frame, value) = frameAndValue;
              var (tangentIn, tangentOut) = tangents;
              scaleTangentKeyframes[frame / fps]
                  = (tangentIn, value, tangentOut);
            }

            gltfAnimation.CreateScaleChannel(node, scaleTangentKeyframes);
          }
        } else {
          boneTracks.Scales.GetAllFrames(translationsOrScales);
          for (var i = 0; i < translationsOrScales.Length; ++i) {
            var time = i / fps;
            scaleKeyframes[time] = translationsOrScales[i];
          }

          gltfAnimation.CreateScaleChannel(node, scaleKeyframes);
        }
      }
    }

    if (animation.HasAnyMeshTracks) {
      foreach (var (finMesh, gltfNode) in gltfNodeByFinMesh) {
        var finMeshTracks = animation.MeshTracks[finMesh];

        var displayStates = finMeshTracks.DisplayStates;
        if (displayStates.HasAnyData) {
          visibilityKeyframes.Clear();
          foreach (var keyframe in displayStates.Definitions) {
            if (keyframe.ValueOut is MeshDisplayState.HIDDEN) {
              visibilityKeyframes[keyframe.Frame] = false;
            } else if (keyframe.ValueOut is MeshDisplayState.VISIBLE) {
              visibilityKeyframes[keyframe.Frame] = true;
            }
          }

          if (visibilityKeyframes.Count > 0) {
            gltfAnimation.CreateVisibilityChannel(
                gltfNode,
                visibilityKeyframes);
          }
        }
      }
    }
  }

}
