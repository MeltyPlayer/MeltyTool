using System.Numerics;

using fin.animation;
using fin.animation.tracks;
using fin.math.interpolation;

namespace fin.model;

public interface IAxes2fTrack<TInterpolated>
    : IAxesTrack<float, TInterpolated> {
  void Set<TVector2>(int frame, float x, float y) {
    Set(frame, 0, x);
    Set(frame, 1, y);
  }

  void Set(int frame, Vector2 values) {
    Set(frame, 0, values.X);
    Set(frame, 1, values.Y);
  }
}

public interface IPosition2dTrack : IAxes2fTrack<Vector2> { }

public interface IRadiansRotation2dTrack
    : IInputOutputTrack<float, RadianInterpolator> { }

public interface IScale2dTrack : IAxes2fTrack<Vector2> { }