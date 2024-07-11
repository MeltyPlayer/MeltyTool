using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.single;
using fin.animation.types.vector3;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class BoneTracksImpl {
    public IVector3Interpolatable? Translations { get; private set; }

    public ISeparateVector3Keyframes<Keyframe<float>>
        UseSeparateTranslationKeyframes(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity) {
      var keyframes = new SeparateVector3Keyframes<Keyframe<float>>(
          sharedConfig,
          FloatKeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialXCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Translation.X),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialYCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Translation.Y),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialZCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Translation.Z),
          });

      this.Translations = keyframes;

      return keyframes;
    }

    public ISeparateVector3Keyframes<KeyframeWithTangents<float>>
        UseSeparateTranslationKeyframesWithTangents(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity) {
      var keyframes = new SeparateVector3Keyframes<KeyframeWithTangents<float>>(
          sharedConfig,
          FloatKeyframeWithTangentsInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialXCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Translation.X),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialYCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Translation.Y),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialZCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Translation.Z),
          });

      this.Translations = keyframes;

      return keyframes;
    }

    public ICombinedVector3Keyframes<Keyframe<Vector3>>
        UseCombinedTranslationKeyframes(int initialCapacity = 0) {
      var keyframes = new CombinedVector3Keyframes<Keyframe<Vector3>>(
          sharedConfig,
          Vector3KeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<Vector3> {
              InitialCapacity = initialCapacity,
              DefaultValue = Optional.Of(() => bone.LocalTransform.Translation),
          });

      this.Translations = keyframes;

      return keyframes;
    }

    public ICombinedVector3Keyframes<KeyframeWithTangents<Vector3>>
        UseCombinedTranslationKeyframesWithTangents(int initialCapacity = 0) {
      var keyframes
          = new CombinedVector3Keyframes<KeyframeWithTangents<Vector3>>(
              sharedConfig,
              Vector3KeyframeWithTangentsInterpolator.Instance,
              new IndividualInterpolationConfig<Vector3> {
                  InitialCapacity = initialCapacity,
                  DefaultValue
                      = Optional.Of(() => bone.LocalTransform.Translation),
              });

      this.Translations = keyframes;

      return keyframes;
    }
  }
}