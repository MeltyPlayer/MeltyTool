using System;

using fin.math.floats;
using fin.math.interpolation;

namespace fin.animation.interpolation;

/// <summary>
///   Helper class for interpolating a track with a variable "mag filter" for
///   higher framerates.
/// </summary>
public class MagFilterInterpolatable<T>(
    IInterpolatable<T> impl,
    IInterpolator<T> interpolator)
    : IInterpolatable<T> {
  public AnimationInterpolationMagFilter AnimationInterpolationMagFilter {
    get;
    set;
  } = AnimationInterpolationMagFilter.ANY_FRAME_RATE;

  public bool TryGetAtFrame(float frame, out T value) {
    var intFrame = (int) frame;
    var frac = frame - intFrame;
    if (frac.IsRoughly0() ||
        this.AnimationInterpolationMagFilter ==
        AnimationInterpolationMagFilter.ORIGINAL_FRAME_RATE_NEAREST) {
      return impl.TryGetAtFrame(intFrame, out value);
    }

    if (this.AnimationInterpolationMagFilter ==
        AnimationInterpolationMagFilter.ANY_FRAME_RATE) {
      return impl.TryGetAtFrame(frame, out value);
    }

    if (impl.TryGetAtFrame(intFrame, out var fromValue) &&
        impl.TryGetAtFrame((int) Math.Ceiling(frame),
                           out var toValue)) {
      value = interpolator.Interpolate(fromValue, toValue, frac);
      return true;
    }

    value = default;
    return false;
  }
}