using System.Numerics;

using fin.animation.keyframes;
using fin.animation.types.single;
using fin.animation.types.vector3;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class BoneTracksImpl {
    public IVector3Interpolatable? Scales { get; private set; }

    public ISeparateVector3Keyframes<Keyframe<float>>
        UseSeparateScaleKeyframes(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity) {
      var keyframes = new SeparateVector3Keyframes<Keyframe<float>>(
          sharedConfig,
          FloatKeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialXCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Scale?.X ?? 1),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialYCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Scale?.Y ?? 1),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialZCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Scale?.Z ?? 1),
          });

      this.Scales = keyframes;

      return keyframes;
    }

    public ISeparateVector3Keyframes<KeyframeWithTangents<float>>
        UseSeparateScaleKeyframesWithTangents(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity) {
      var keyframes = new SeparateVector3Keyframes<KeyframeWithTangents<float>>(
          sharedConfig,
          FloatKeyframeWithTangentsInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialXCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Scale?.X ?? 0),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialYCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Scale?.Y ?? 0),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialZCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Scale?.Z ?? 0),
          });

      this.Scales = keyframes;

      return keyframes;
    }

    public ICombinedVector3Keyframes<Keyframe<Vector3>>
        UseCombinedScaleKeyframes(int initialCapacity = 0) {
      var keyframes = new CombinedVector3Keyframes<Keyframe<Vector3>>(
          sharedConfig,
          Vector3KeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<Vector3> {
              InitialCapacity = initialCapacity,
              DefaultValue
                  = Optional.Of(() => bone.LocalTransform.Scale ?? Vector3.One),
          });

      this.Scales = keyframes;

      return keyframes;
    }

    public ICombinedVector3Keyframes<KeyframeWithTangents<Vector3>>
        UseCombinedScaleKeyframesWithTangents(int initialCapacity = 0) {
      var keyframes
          = new CombinedVector3Keyframes<KeyframeWithTangents<Vector3>>(
              sharedConfig,
              Vector3KeyframeWithTangentsInterpolator.Instance,
              new IndividualInterpolationConfig<Vector3> {
                  InitialCapacity = initialCapacity,
                  DefaultValue
                      = Optional.Of(
                          () => bone.LocalTransform.Scale ?? Vector3.One),
              });

      this.Scales = keyframes;

      return keyframes;
    }
  }
}