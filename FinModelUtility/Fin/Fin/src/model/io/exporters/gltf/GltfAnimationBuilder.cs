using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.data.indexable;

using SharpGLTF.Schema2;

namespace fin.model.io.exporters.gltf;

using GltfNode = Node;

public class GltfAnimationBuilder {
  public void BuildAnimations(
      ModelRoot gltfModel,
      (GltfNode, IReadOnlyBone)[] skinNodesAndBones,
      float modelScale,
      IReadOnlyList<IReadOnlyModelAnimation> animations) {
    foreach (var animation in animations) {
      this.BuildAnimation_(gltfModel, skinNodesAndBones, modelScale, animation);
    }
  }

  private void BuildAnimation_(
      ModelRoot gltfModel,
      (GltfNode, IReadOnlyBone)[] skinNodesAndBones,
      float modelScale,
      IReadOnlyModelAnimation animation) {
    var isValid
        = animation
          .BoneTracks
          .Any(finBoneTracks
                   => (finBoneTracks.Translations?.HasAnyData ?? false) ||
                      (finBoneTracks.Rotations?.HasAnyData ?? false) ||
                      (finBoneTracks.Scales?.HasAnyData ?? false));

    if (!isValid) {
      return;
    }

    var gltfAnimation = gltfModel.UseAnimation(animation.Name);

    var fps = animation.FrameRate;

    // Writes translation/rotation/scale for each joint.
    var translationKeyframes = new Dictionary<float, Vector3>();
    var rotationKeyframes = new Dictionary<float, Quaternion>();
    var scaleKeyframes = new Dictionary<float, Vector3>();

    Span<Vector3> translationsOrScales
        = stackalloc Vector3[animation.FrameCount];
    Span<Quaternion> rotations = stackalloc Quaternion[animation.FrameCount];

    foreach (var (node, bone) in skinNodesAndBones) {
      if (!animation.BoneTracks.TryGetValue(bone, out var boneTracks)) {
        continue;
      }

      // TODO: How to get keyframes for sparse tracks?

      var translationDefined = boneTracks.Translations?.HasAnyData ?? false;
      if (translationDefined) {
        translationKeyframes.Clear();
        boneTracks.Translations.GetAllFrames(translationsOrScales);
        for (var i = 0; i < translationsOrScales.Length; ++i) {
          var time = i / fps;
          translationKeyframes[time] = translationsOrScales[i] * modelScale;
        }

        gltfAnimation.CreateTranslationChannel(node, translationKeyframes);
      }

      var rotationDefined = boneTracks.Rotations?.HasAnyData ?? false;
      if (rotationDefined) {
        rotationKeyframes.Clear();
        boneTracks.Rotations.GetAllFrames(rotations);
        for (var i = 0; i < animation.FrameCount; ++i) {
          var time = i / fps;
          rotationKeyframes[time] = rotations[i];
        }

        gltfAnimation.CreateRotationChannel(node, rotationKeyframes);
      }


      var scaleDefined = boneTracks.Scales?.HasAnyData ?? false;
      if (scaleDefined) {
        scaleKeyframes.Clear();
        boneTracks.Scales.GetAllFrames(translationsOrScales);
        for (var i = 0; i < translationsOrScales.Length; ++i) {
          var time = i / fps;
          scaleKeyframes[time] = translationsOrScales[i];
        }

        gltfAnimation.CreateScaleChannel(node, scaleKeyframes);
      }
    }
  }
}