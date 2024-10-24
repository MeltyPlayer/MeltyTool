using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector2;

public class SeparateVector2Keyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, float> interpolator,
    IndividualInterpolationConfig<float> individualConfigX,
    IndividualInterpolationConfig<float> individualConfigY)
    : ISeparateVector2Keyframes<TKeyframe>
    where TKeyframe : IKeyframe<float> {
  public SeparateVector2Keyframes(
      ISharedInterpolationConfig sharedConfig,
      IKeyframeInterpolator<TKeyframe, float> interpolator,
      IndividualInterpolationConfig<float> individualConfig = default)
      : this(sharedConfig,
             interpolator,
             individualConfig,
             individualConfig) { }

  public IReadOnlyList<IInterpolatableKeyframes<TKeyframe, float>> Axes { get; }
    = [
        new InterpolatedKeyframes<TKeyframe, float>(
            sharedConfig,
            interpolator,
            individualConfigX),
        new InterpolatedKeyframes<TKeyframe, float>(
            sharedConfig,
            interpolator,
            individualConfigY),
    ];

  public bool TryGetAtFrame(float frame, out Vector2 value) {
    value = default;

    if (!this.Axes[0].TryGetAtFrameOrDefault(frame, individualConfigX, out var x)) {
      return false;
    }

    if (!this.Axes[1].TryGetAtFrameOrDefault(frame, individualConfigY, out var y)) {
      return false;
    }

    value = new Vector2(x, y);
    return true;
  }

  public void GetAllFrames(Span<Vector2> dst) {
    Span<float> x = stackalloc float[dst.Length];
    Span<float> y = stackalloc float[dst.Length];

    this.Axes[0].GetAllFrames(x);
    this.Axes[1].GetAllFrames(y);

    for (var i = 0; i < dst.Length; ++i) {
      dst[i] = new Vector2(x[i], y[i]);
    }
  }
}