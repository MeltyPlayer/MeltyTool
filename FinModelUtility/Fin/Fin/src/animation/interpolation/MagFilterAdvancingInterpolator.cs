using System;

using fin.math.floats;
using fin.math.interpolation;

namespace fin.animation.interpolation;

public sealed class MagFilterAdvancingInterpolator<T>(
    MagFilterInterpolatable<T> parent,
    IInterpolator<T> interpolator)
    : IAdvancingInterpolator<T> {
  private IAdvancingInterpolator<T> fromFrameInterpolator_
      = parent.Impl.GetAdvancingInterpolator();

  private IAdvancingInterpolator<T> toFrameInterpolator_
      = parent.Impl.GetAdvancingInterpolator();

  public bool TryGetAtFrame(float frame, out T value) {
    var animationInterpolationMagFilter
        = parent.AnimationInterpolationMagFilter;

    if (animationInterpolationMagFilter ==
        AnimationInterpolationMagFilter.ANY_FRAME_RATE) {
      return this.fromFrameInterpolator_.TryGetAtFrame(frame, out value);
    }

    var intFrame = (int) frame;
    var frac = frame - intFrame;
    if (frac.IsRoughly0() ||
        animationInterpolationMagFilter ==
        AnimationInterpolationMagFilter.ORIGINAL_FRAME_RATE_NEAREST) {
      return this.fromFrameInterpolator_.TryGetAtFrame(intFrame, out value);
    }

    if (this.fromFrameInterpolator_.TryGetAtFrame(
            intFrame,
            out var fromValue) &&
        this.toFrameInterpolator_.TryGetAtFrame((int) Math.Ceiling(frame),
                                                out var toValue)) {
      value = interpolator.Interpolate(fromValue, toValue, frac);
      return true;
    }

    value = default!;
    return false;
  }
}