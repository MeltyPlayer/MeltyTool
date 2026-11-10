using System.Collections.Generic;
using fin.animation.keyframes;

namespace fin.animation.interpolation;

public sealed class StairStepAdvancingInterpolator<T>(
  ISharedInterpolationConfig sharedConfig,
  IIndividualInterpolationConfig<T>? individualConfig,
  IReadOnlyList<Keyframe<T>> keyframes)
  : IAdvancingInterpolator<T> {
  private readonly AdvancingInterpolator<T, Keyframe<T>> impl_ = new(
    sharedConfig,
    individualConfig,
    keyframes,
    null!);

  public bool TryGetAtFrame(float frame, out T value) {
    if (this.impl_.TryGetPrecedingAndFollowingKeyframes(
          frame,
          out var keyframe,
          out _) is not KeyframesUtil.InterpolationDataType.NONE) {
      value = keyframe.ValueOut;
      return true;
    }

    value = default!;
    return false;
  }

  // TODO: Ugh... this should not be here.
  public bool TryGetPrecedingAndFollowingValues(
    float frame,
    out T preceding,
    out T following,
    out float fraction) {
    if (this.impl_.TryGetPrecedingAndFollowingKeyframes(
          frame,
          out var precedingKeyframe,
          out var followingKeyframe,
          out fraction) is not KeyframesUtil.InterpolationDataType.NONE) {
      preceding = precedingKeyframe.ValueOut;
      following = followingKeyframe.ValueIn;
      return true;
    }

    preceding = following = default!;
    return false;
  }
}