using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.single;
using fin.animation.types.vector3;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class TextureTracksImpl {
    public ISeparateVector3Keyframes<KeyframeWithTangents<float>>?
        Translations { get; private set; }

    public ISeparateVector3Keyframes<KeyframeWithTangents<float>>
        UseSeparateTranslationKeyframesWithTangents(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity,
            int? animationLength = null) {
      var keyframes = new SeparateVector3Keyframes<KeyframeWithTangents<float>>(
          sharedConfig,
          FloatKeyframeWithTangentsInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              AnimationLength = animationLength,
              InitialCapacity = initialXCapacity,
              DefaultValue
                  = Optional.Of(() => texture.Translation?.X ?? 0),
          },
          new IndividualInterpolationConfig<float> {
              AnimationLength = animationLength,
              InitialCapacity = initialYCapacity,
              DefaultValue
                  = Optional.Of(() => texture.Translation?.Y ?? 0),
          },
          new IndividualInterpolationConfig<float> {
              AnimationLength = animationLength,
              InitialCapacity = initialZCapacity,
              DefaultValue
                  = Optional.Of(() => texture.Translation?.Z ?? 0),
          });

      this.Translations = keyframes;

      return keyframes;
    }
  }
}