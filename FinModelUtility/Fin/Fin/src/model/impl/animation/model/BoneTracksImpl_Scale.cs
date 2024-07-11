using fin.animation.keyframes;
using fin.animation.types.single;
using fin.animation.types.vector3;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class BoneTracksImpl {
    public IVector3Interpolatable? Scales { get; private set; }

    public ISeparateVector3Keyframes<Keyframe<float>>
        UseSeparateScaleAxesTrack(
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
        UseSeparateScaleAxesTrackWithTangents(
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
  }
}