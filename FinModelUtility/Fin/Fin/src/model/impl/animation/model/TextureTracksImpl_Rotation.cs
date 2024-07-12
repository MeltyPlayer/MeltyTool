using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.single;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class TextureTracksImpl {
    public IInterpolatable<float>? Rotations { get; private set; }

    public IInterpolatableKeyframes<Keyframe<float>, float>
        UseRotationKeyframes(int initialCapacity) {
      var keyframes = new InterpolatedKeyframes<Keyframe<float>, float>(
          sharedConfig,
          FloatKeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialCapacity,
              DefaultValue = Optional.Of(() => texture.RotationRadians?.X ?? 0),
          });

      this.Rotations = keyframes;

      return keyframes;
    }

    public IInterpolatableKeyframes<KeyframeWithTangents<float>, float>
        UseRotationKeyframesWithTangents(int initialCapacity) {
      var keyframes = new InterpolatedKeyframes<KeyframeWithTangents<float>, float>(
          sharedConfig,
          FloatKeyframeWithTangentsInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialCapacity,
              DefaultValue = Optional.Of(() => texture.RotationRadians?.X ?? 0),
          });

      this.Rotations = keyframes;

      return keyframes;
    }
  }
}