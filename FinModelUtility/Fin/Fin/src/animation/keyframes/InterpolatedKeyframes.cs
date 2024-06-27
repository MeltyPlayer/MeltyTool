using System.Collections.Generic;

using fin.animation.interpolation;
using fin.util.optional;

namespace fin.animation.keyframes;

public interface IInterpolatableKeyframes<TKeyframe, T>
    : IKeyframes<TKeyframe>,
      IInterpolatable<T> where TKeyframe : IKeyframe<T>;

public readonly struct IndividualInterpolationConfig<T>() {
  public int InitialCapacity { get; init; } = 0;
  public IOptional<T>? DefaultValue { get; init; } = null;
}

public class InterpolatedKeyframes<TKeyframe, T>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, T> interpolator,
    IndividualInterpolationConfig<T> individualConfig = default)
    : IInterpolatableKeyframes<TKeyframe, T>
    where TKeyframe : IKeyframe<T> {
  private readonly List<TKeyframe>
      impl_ = new(individualConfig.InitialCapacity);

  public IReadOnlyList<TKeyframe> Definitions => this.impl_;

  public void Add(TKeyframe keyframe) => this.impl_.AddKeyframe(keyframe);

  public bool TryGetAtFrame(float frame, out T value) {
    switch (this.impl_.TryGetPrecedingAndFollowingKeyframes(
                frame,
                sharedConfig,
                out var precedingKeyframe,
                out var followingKeyframe,
                out var normalizedFrame)) {
      case KeyframesUtil.InterpolationDataType.PRECEDING_AND_FOLLOWING:
        value = interpolator.Interpolate(precedingKeyframe,
                                         followingKeyframe,
                                         normalizedFrame,
                                         sharedConfig);
        return true;

      case KeyframesUtil.InterpolationDataType.PRECEDING_ONLY:
        value = precedingKeyframe.Value;
        return true;

      default:
      case KeyframesUtil.InterpolationDataType.NONE:
        if (individualConfig.DefaultValue?.Try(out value) ?? false) {
          return true;
        }

        value = default;
        return false;
    }
  }
}