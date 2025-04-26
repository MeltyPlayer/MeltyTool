using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.quaternion;
using fin.animation.types.radians;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class TextureTracksImpl {
    public ISeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>?
        Rotations { get; private set; }

    public ISeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>
        UseSeparateRotationKeyframesWithTangents(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity,
            int? animationLength = null) {
      var keyframes
          = new SeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>(
              sharedConfig,
              RadiansKeyframeWithTangentsInterpolator.Instance,
              new IndividualInterpolationConfig<float> {
                  AnimationLength = animationLength,
                  InitialCapacity = initialXCapacity,
                  DefaultValue
                      = Optional.Of(() => texture.RotationRadians?.X ?? 0),
              },
              new IndividualInterpolationConfig<float> {
                  AnimationLength = animationLength,
                  InitialCapacity = initialYCapacity,
                  DefaultValue
                      = Optional.Of(() => texture.RotationRadians?.Y ?? 0),
              },
              new IndividualInterpolationConfig<float> {
                  AnimationLength = animationLength,
                  InitialCapacity = initialZCapacity,
                  DefaultValue
                      = Optional.Of(() => texture.RotationRadians?.Z ?? 0),
              });

      this.Rotations = keyframes;

      return keyframes;
    }
  }
}