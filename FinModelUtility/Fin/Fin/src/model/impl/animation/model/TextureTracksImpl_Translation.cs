using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.single;
using fin.animation.types.vector2;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class TextureTracksImpl {
    public IVector2Interpolatable? Translations { get; private set; }

    public ISeparateVector2Keyframes<Keyframe<float>>
        UseSeparateTranslationKeyframes(
            int initialXCapacity,
            int initialYCapacity) {
      var keyframes = new SeparateVector2Keyframes<Keyframe<float>>(
          sharedConfig,
          FloatKeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialXCapacity,
              DefaultValue = Optional.Of(() => texture.Offset?.X ?? 0),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialYCapacity,
              DefaultValue = Optional.Of(() => texture.Offset?.Y ?? 0),
          });

      this.Translations = keyframes;

      return keyframes;
    }

    public ISeparateVector2Keyframes<KeyframeWithTangents<float>>
        UseSeparateTranslationKeyframesWithTangents(
            int initialXCapacity,
            int initialYCapacity) {
      var keyframes = new SeparateVector2Keyframes<KeyframeWithTangents<float>>(
          sharedConfig,
          FloatKeyframeWithTangentsInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialXCapacity,
              DefaultValue = Optional.Of(() => texture.Offset?.X ?? 0),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialYCapacity,
              DefaultValue = Optional.Of(() => texture.Offset?.Y ?? 0),
          });

      this.Translations = keyframes;

      return keyframes;
    }

    public ICombinedVector2Keyframes<Keyframe<Vector2>>
        UseCombinedTranslationKeyframes(int initialCapacity = 0) {
      var keyframes = new CombinedVector2Keyframes<Keyframe<Vector2>>(
          sharedConfig,
          Vector2KeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<Vector2> {
              InitialCapacity = initialCapacity,
              DefaultValue
                  = Optional.Of(() => new Vector2(
                                    texture.Offset?.X ?? 0,
                                    texture.Offset?.Y ?? 0)),
          });

      this.Translations = keyframes;

      return keyframes;
    }

    public ICombinedVector2Keyframes<KeyframeWithTangents<Vector2>>
        UseCombinedTranslationKeyframesWithTangents(int initialCapacity = 0) {
      var keyframes
          = new CombinedVector2Keyframes<KeyframeWithTangents<Vector2>>(
              sharedConfig,
              Vector2KeyframeWithTangentsInterpolator.Instance,
              new IndividualInterpolationConfig<Vector2> {
                  InitialCapacity = initialCapacity,
                  DefaultValue
                      = Optional.Of(() => new Vector2(
                                        texture.Offset?.X ?? 0,
                                        texture.Offset?.Y ?? 0)),
              });

      this.Translations = keyframes;

      return keyframes;
    }
  }
}