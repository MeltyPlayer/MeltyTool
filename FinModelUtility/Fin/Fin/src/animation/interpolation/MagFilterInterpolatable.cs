using System;
using System.Collections.Generic;

using fin.math.floats;
using fin.math.interpolation;

namespace fin.animation.interpolation;

/// <summary>
///   Helper class for interpolating a track with a variable "mag filter" for
///   higher framerates.
/// </summary>
public sealed class MagFilterInterpolatable<T>(IInterpolator<T> interpolator)
    : IInterpolatable<T> {
  public IInterpolatable<T>? Impl { get; set; }
  public bool HasAnyData => this.Impl?.HasAnyData ?? false;

  public int AnimationLength => this.Impl?.AnimationLength ?? 0;
  public bool Looping => this.Impl?.Looping ?? false;

  public bool TryGetAtFrameWithBinarySearch(float frame, out T value) {
    if (this.Impl == null) {
      value = default!;
      return false;
    }

    var intFrame = (int) frame;
    var frac = frame - intFrame;
    if (frac.IsRoughly0()) {
      return this.Impl.TryGetAtFrameWithBinarySearch(intFrame, out value);
    }

    if (this.Impl.TryGetAtFrameWithBinarySearch(intFrame, out var fromValue) &&
        this.Impl.TryGetAtFrameWithBinarySearch((int) Math.Ceiling(frame),
                                                out var toValue)) {
      value = interpolator.Interpolate(fromValue, toValue, frac);
      return true;
    }

    value = default!;
    return false;
  }

  public void GetAllFrames(Span<T> dst) => this.Impl.GetAllFrames(dst);

  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, T value)> keyframes,
      out IReadOnlyList<(T tangentIn, T tangentOut)>? tangentKeyframes)
    => this.Impl.TryGetSimpleKeyframes(out keyframes, out tangentKeyframes);

  public IAdvancingInterpolator<T> GetAdvancingInterpolator()
    => this.Looping
        ? new LoopingMagFilterAdvancingInterpolator<T>(this, interpolator)
        : new NonLoopingMagFilterAdvancingInterpolator<T>(this, interpolator);
}