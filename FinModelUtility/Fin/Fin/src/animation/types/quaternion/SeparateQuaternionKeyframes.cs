using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.quaternion;

public class SeparateQuaternionKeyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, float> interpolator,
    IndividualInterpolationConfig<float> individualConfigX,
    IndividualInterpolationConfig<float> individualConfigY,
    IndividualInterpolationConfig<float> individualConfigZ,
    IndividualInterpolationConfig<float> individualConfigW)
    : ISeparateQuaternionKeyframes<TKeyframe>
    where TKeyframe : IKeyframe<float> {
  public SeparateQuaternionKeyframes(
      ISharedInterpolationConfig sharedConfig,
      IKeyframeInterpolator<TKeyframe, float> interpolator,
      IndividualInterpolationConfig<float> individualConfig = default)
      : this(sharedConfig,
             interpolator,
             individualConfig,
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
        new InterpolatedKeyframes<TKeyframe, float>(
            sharedConfig,
            interpolator,
            individualConfigW),
    ];

  public bool TryGetAtFrame(float frame, out Quaternion value) {
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

    if (!this.Axes[3].TryGetAtFrameOrDefault(frame, individualConfigW, out var w)) {
      return false;
    }

    value = new Quaternion(x, y, z, w);
    return true;
  }
}