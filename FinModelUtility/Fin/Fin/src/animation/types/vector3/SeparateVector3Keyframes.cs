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
    if (this.Axes[0].TryGetAtFrame(frame, out var x) &&
        this.Axes[1].TryGetAtFrame(frame, out var y) &&
        this.Axes[2].TryGetAtFrame(frame, out var z)) {
      value = new Vector3(x, y, z);
      return true;
    }

    value = default;
    return false;
  }
}