using System;

using fin.math.floats;
using fin.model;

namespace fin.animation {
  /// <summary>
  ///   Helper class for interpolating a track with a variable "mag filter" for
  ///   higher framerates.
  /// </summary>
  public class MagFilterInterpolationTrack<T>(
      IReadOnlyInterpolatedTrack<T> impl,
      Func<T, T, float, T> interpolator)
      : IReadOnlyInterpolatedTrack<T> {
    public IReadOnlyInterpolatedTrack<T> Impl { get; set; } = impl;

    public AnimationInterpolationMagFilter AnimationInterpolationMagFilter { get; set; } =
      AnimationInterpolationMagFilter.ANY_FRAME_RATE;

    public IAnimation Animation => this.Impl.Animation;
    public bool HasAtLeastOneKeyframe => this.Impl.HasAtLeastOneKeyframe;
    public int MaxKeyframe => this.Impl.MaxKeyframe;

    public bool TryGetInterpolatedFrame(float frame, out T interpolatedValue, AnimationInterpolationConfig? config = null) {
      var intFrame = (int) frame;
      var frac = frame - intFrame;
      if (frac.IsRoughly0() || this.AnimationInterpolationMagFilter == AnimationInterpolationMagFilter.ORIGINAL_FRAME_RATE_NEAREST) {
        return this.Impl.TryGetInterpolatedFrame(intFrame, out interpolatedValue, config);
      }

      switch (this.AnimationInterpolationMagFilter) {
        case AnimationInterpolationMagFilter.ORIGINAL_FRAME_RATE_LINEAR: {
            if (this.Impl.TryGetInterpolatedFrame(intFrame, out var fromValue, config) &&
                this.Impl.TryGetInterpolatedFrame((int) Math.Ceiling(frame), out var toValue, config)) {
              interpolatedValue = interpolator(fromValue, toValue, frac);
              return true;
            }

            interpolatedValue = default;
            return false;
          }
        case AnimationInterpolationMagFilter.ANY_FRAME_RATE:
        default: {
            return this.Impl.TryGetInterpolatedFrame(frame, out interpolatedValue, config);
          }
      }
    }
  }
}