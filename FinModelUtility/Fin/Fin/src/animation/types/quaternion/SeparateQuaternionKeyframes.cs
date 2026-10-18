using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.vector3;
using fin.util.asserts;

namespace fin.animation.types.quaternion;

public sealed class SeparateQuaternionKeyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, float> interpolator,
    IndividualInterpolationConfig<float>? individualConfigX,
    IndividualInterpolationConfig<float>? individualConfigY,
    IndividualInterpolationConfig<float>? individualConfigZ,
    IndividualInterpolationConfig<float>? individualConfigW)
    : ISeparateQuaternionKeyframes<TKeyframe>
    where TKeyframe : class, IKeyframe<float> {
  public SeparateQuaternionKeyframes(
      ISharedInterpolationConfig sharedConfig,
      IKeyframeInterpolator<TKeyframe, float> interpolator,
      IndividualInterpolationConfig<float>? individualConfig = null)
      : this(sharedConfig,
             interpolator,
             individualConfig,
             individualConfig,
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
        new InterpolatedKeyframes<TKeyframe, float>(
            sharedConfig,
            interpolator,
            individualConfigZ),
        new InterpolatedKeyframes<TKeyframe, float>(
            sharedConfig,
            interpolator,
            individualConfigW),
    ];

  public bool TryGetAtFrameWithBinarySearch(float frame, out Quaternion value) {
    value = default;

    if (!this.Axes[0]
             .TryGetAtFrameOrDefault(frame, individualConfigX, out var x)) {
      return false;
    }

    if (!this.Axes[1]
             .TryGetAtFrameOrDefault(frame, individualConfigY, out var y)) {
      return false;
    }

    if (!this.Axes[2]
             .TryGetAtFrameOrDefault(frame, individualConfigZ, out var z)) {
      return false;
    }

    if (!this.Axes[3]
             .TryGetAtFrameOrDefault(frame, individualConfigW, out var w)) {
      return false;
    }

    value = new Quaternion(x, y, z, w);
    return true;
  }

  public void GetAllFrames(Span<Quaternion> dst) {
    Span<float> x = stackalloc float[dst.Length];
    Span<float> y = stackalloc float[dst.Length];
    Span<float> z = stackalloc float[dst.Length];
    Span<float> w = stackalloc float[dst.Length];

    this.Axes[0].GetAllFrames(x);
    this.Axes[1].GetAllFrames(y);
    this.Axes[2].GetAllFrames(z);
    this.Axes[3].GetAllFrames(w);

    for (var i = 0; i < dst.Length; ++i) {
      dst[i] = new Quaternion(x[i], y[i], z[i], w[i]);
    }
  }

  // TODO: Implement this
  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, Quaternion value)> keyframes,
      out IReadOnlyList<(Quaternion tangentIn, Quaternion tangentOut)>?
          tangentKeyframes) {
    keyframes = null;
    tangentKeyframes = null;
    return false;
  }

  public IAdvancingInterpolator<Quaternion> GetAdvancingInterpolator()
    => new SeparateQuaternionAdvancingInterpolator<TKeyframe>(
        sharedConfig,
        individualConfigX,
        individualConfigY,
        individualConfigZ,
        individualConfigW,
        this.Axes[0].AssertAsA<InterpolatedKeyframes<TKeyframe, float>>().Impl,
        this.Axes[1].AssertAsA<InterpolatedKeyframes<TKeyframe, float>>().Impl,
        this.Axes[2].AssertAsA<InterpolatedKeyframes<TKeyframe, float>>().Impl,
        this.Axes[3].AssertAsA<InterpolatedKeyframes<TKeyframe, float>>().Impl,
        interpolator);
}