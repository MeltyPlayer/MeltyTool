﻿using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.single;
using fin.animation.types.vector3;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class TextureTracksImpl {
    public ISeparateVector3Keyframes<KeyframeWithTangents<float>>?
        Scales { get; private set; }

    public ISeparateVector3Keyframes<KeyframeWithTangents<float>>
        UseSeparateScaleKeyframesWithTangents(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity,
            int? animationLength = null) {
      var transform = texture.TextureTransform;
      var keyframes = new SeparateVector3Keyframes<KeyframeWithTangents<float>>(
          sharedConfig,
          FloatKeyframeWithTangentsInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              AnimationLength = animationLength,
              InitialCapacity = initialXCapacity,
              DefaultValue
                  = Optional.Of(() => transform.Scale?.X ?? 0),
          },
          new IndividualInterpolationConfig<float> {
              AnimationLength = animationLength,
              InitialCapacity = initialYCapacity,
              DefaultValue
                  = Optional.Of(() => transform.Scale?.Y ?? 0),
          },
          new IndividualInterpolationConfig<float> {
              AnimationLength = animationLength,
              InitialCapacity = initialZCapacity,
              DefaultValue
                  = Optional.Of(() => transform.Scale?.Z ?? 0),
          });

      this.Scales = keyframes;

      return keyframes;
    }
  }
}