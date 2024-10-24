using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector3;

public class SeparateVector3Keyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, float> interpolator,
    IndividualInterpolationConfig<float> individualConfigX,
    IndividualInterpolationConfig<float> individualConfigY,
    IndividualInterpolationConfig<float> individualConfigZ)
    : ISeparateVector3Keyframes<TKeyframe>
    where TKeyframe : IKeyframe<float> {
  public SeparateVector3Keyframes(
      ISharedInterpolationConfig sharedConfig,
      IKeyframeInterpolator<TKeyframe, float> interpolator,
      IndividualInterpolationConfig<float> individualConfig = default)
      : this(sharedConfig,
             interpolator,
             individualConfig,
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
        new InterpolatedKeyframes<TKeyframe, float>(
            sharedConfig,
            interpolator,
            individualConfigZ),
    ];

  public bool TryGetAtFrame(float frame, out Vector3 value) {
    value = default;

    if (!this.Axes[0].TryGetAtFrameOrDefault(frame, individualConfigX, out var x)) {
      return false;
    }

    if (!this.Axes[1].TryGetAtFrameOrDefault(frame, individualConfigY, out var y)) {
      return false;
    }

    if (!this.Axes[2].TryGetAtFrameOrDefault(frame, individualConfigZ, out var z)) {
      return false;
    }

    value = new Vector3(x, y, z);
    return true;
  }

  public void GetAllFrames(Span<Vector3> dst) {
    Span<float> x = stackalloc float[dst.Length];
    Span<float> y = stackalloc float[dst.Length];
    Span<float> z = stackalloc float[dst.Length];

    this.Axes[0].GetAllFrames(x);
    this.Axes[1].GetAllFrames(y);
    this.Axes[2].GetAllFrames(z);

    for (var i = 0; i < dst.Length; ++i) {
      dst[i] = new Vector3(x[i], y[i], z[i]);
    }
  }
}