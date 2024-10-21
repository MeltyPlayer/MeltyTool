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
      var isValid
          = animation
            .BoneTracks
            .Any(finBoneTracks
                     => (finBoneTracks.Translations?.HasAnyData ?? false) ||
                        (finBoneTracks.Rotations?.HasAnyData ?? false) ||
                        (finBoneTracks.Scales?.HasAnyData ?? false));

      if (!isValid) {
        continue;
      }

      var gltfAnimation = gltfModel.UseAnimation(animation.Name);

      var fps = animation.FrameRate;

      // Writes translation/rotation/scale for each joint.
      var translationKeyframes = new Dictionary<float, Vector3>();
      var rotationKeyframes = new Dictionary<float, Quaternion>();
      var scaleKeyframes = new Dictionary<float, Vector3>();
      foreach (var (node, bone) in skinNodesAndBones) {
        if (!animation.BoneTracks.TryGetValue(bone, out var boneTracks)) {
          continue;
        }

        translationKeyframes.Clear();
        rotationKeyframes.Clear();
        scaleKeyframes.Clear();

        var translationDefined = boneTracks.Translations?.HasAnyData ?? false;
        var rotationDefined = boneTracks.Rotations?.HasAnyData ?? false;
        var scaleDefined = boneTracks.Scales?.HasAnyData ?? false;

        // TODO: How to get keyframes for sparse tracks?
        for (var i = 0; i < animation.FrameCount; ++i) {
          var time = i / fps;

          if (translationDefined) {
            if (boneTracks.Translations.TryGetAtFrame(i, out var translation)) {
              translationKeyframes[time] = translation * modelScale;
            }
          }

          if (rotationDefined) {
            if (boneTracks.Rotations.TryGetAtFrame(i, out var rotation)) {
              rotationKeyframes[time] = rotation;
            }
          }

          if (scaleDefined) {
            boneTracks.Scales.TryGetAtFrame(i, out var scale);
            scaleKeyframes[time] = new Vector3(scale.X, scale.Y, scale.Z);
          }
        }

        if (translationDefined) {
          gltfAnimation.CreateTranslationChannel(node, translationKeyframes);
        }

        if (rotationDefined) {
          gltfAnimation.CreateRotationChannel(node, rotationKeyframes);
        }

        if (scaleDefined) {
          gltfAnimation.CreateScaleChannel(node, scaleKeyframes);
        }
      }
    }
  }
}