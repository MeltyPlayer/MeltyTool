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
    if (this.impl_.TryToGetPrecedingKeyframe(
            frame,
            out var keyframe,
            out _,
            out _)) {
      value = keyframe.ValueOut;
      return true;
    }

    value = default!;
    return false;
  }
}