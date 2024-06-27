using System.Numerics;

using fin.animation;
using fin.animation.tracks;
using fin.math.interpolation;
using fin.math.xyz;

namespace fin.model;

public interface IAxes3fTrack<TInterpolated>
    : IAxesTrack<float, TInterpolated> {
  void Set<TVector2>(int frame, float x, float y, float z) {
    Set(frame, 0, x);
    Set(frame, 1, y);
    Set(frame, 2, z);
  }

  void Set<TVector3>(int frame, TVector3 values) where TVector3 :
      IReadOnlyXyz {
    Set(frame, 0, values.X);
    Set(frame, 1, values.Y);
    Set(frame, 2, values.Z);
  }

  void Set(int frame, Vector3 values) {
    Set(frame, 0, values.X);
    Set(frame, 1, values.Y);
    Set(frame, 2, values.Z);
  }
}

public interface IPositionTrack3d : IReadOnlyInterpolatedTrack<Vector3>,
                                    IAnimationData { }

public interface ICombinedPositionAxesTrack3d
    : IPositionTrack3d,
      IInputOutputTrack<Vector3, Vector3Interpolator> { }

public interface ISeparatePositionAxesTrack3d : IPositionTrack3d,
                                                IAxes3fTrack<Vector3> { }

public interface IRotationTrack3d : IReadOnlyInterpolatedTrack<Quaternion>,
                                    IAnimationData { }

public interface IQuaternionRotationTrack3d
    : IRotationTrack3d,
      IInputOutputTrack<Quaternion, QuaternionInterpolator> { }

public interface IQuaternionAxesRotationTrack3d
    : IRotationTrack3d,
      IAxesTrack<float, Quaternion> { }


public interface IEulerRadiansRotationTrack3d : IRotationTrack3d,
                                                IAxes3fTrack<Quaternion> {
  // TODO: Slow! Switch to using generics/structs for a speedup here
  ConvertRadiansToQuaternion ConvertRadiansToQuaternionImpl { get; set; }

  delegate Quaternion ConvertRadiansToQuaternion(float xRadians,
                                                 float yRadians,
                                                 float zRadians);
}

public interface IScale3dTrack : IAxes3fTrack<Vector3>,
                                 IAnimationData { }