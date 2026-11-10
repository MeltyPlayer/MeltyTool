using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.util.asserts;

namespace fin.animation.types.vector2;

public sealed class SeparateVector2Keyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, float> interpolator,
    IndividualInterpolationConfig<float>? individualConfigX,
    IndividualInterpolationConfig<float>? individualConfigY)
    : ISeparateVector2Keyframes<TKeyframe>
    where TKeyframe : class, IKeyframe<float> {
  public SeparateVector2Keyframes(
      ISharedInterpolationConfig sharedConfig,
      IKeyframeInterpolator<TKeyframe, float> interpolator,
      IndividualInterpolationConfig<float>? individualConfig = null)
      : this(sharedConfig,
             interpolator,
             individualConfig,
             individualConfig) { }

  public int AnimationLength => sharedConfig.AnimationLength;
  public bool Looping => sharedConfig.Looping;

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

  public bool TryGetAtFrameWithBinarySearch(float frame, out Vector2 value) {
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

  // TODO: Implement this
  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, Vector2 value)> keyframes,
      out IReadOnlyList<(Vector2 tangentIn, Vector2 tangentOut)>? tangentKeyframes) {
    keyframes = null;
    tangentKeyframes = null;
    return false;
  }

  public IAdvancingInterpolator<Vector2> GetAdvancingInterpolator()
    => new SeparateVector2AdvancingInterpolator<TKeyframe>(
        sharedConfig,
        individualConfigX,
        individualConfigY,
        this.Axes[0].AssertAsA<InterpolatedKeyframes<TKeyframe, float>>().Impl,
        this.Axes[1].AssertAsA<InterpolatedKeyframes<TKeyframe, float>>().Impl,
        interpolator);
}