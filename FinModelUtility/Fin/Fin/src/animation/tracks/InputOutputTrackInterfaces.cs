using fin.math.interpolation;

namespace fin.animation.tracks;

public interface IInputOutputTrack<T, TInterpolator>
    : IInputOutputTrack<T, T, TInterpolator>
    where TInterpolator : IInterpolator<T> { }

public interface IInputOutputTrack<TValue, TInterpolated, TInterpolator>
    : IReadOnlyInterpolatedTrack<TInterpolated>,
      IImplTrack<TValue>
    where TInterpolator : IInterpolator<TValue, TInterpolated> {
  TInterpolator Interpolator { get; }

  void Set(IInputOutputTrack<TValue, TInterpolated, TInterpolator> other) { }

  // TODO: Allow setting tangent(s) at each frame.
  // TODO: Allow setting easing at each frame.
  // TODO: Split getting into exactly at frame and interpolated at frame.
  // TODO: Allow getting at fractional frames.
}