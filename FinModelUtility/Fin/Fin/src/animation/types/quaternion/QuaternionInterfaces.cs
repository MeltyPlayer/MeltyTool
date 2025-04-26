using System.Numerics;

using fin.animation.keyframes;
using fin.animation.types.radians;

namespace fin.animation.types.quaternion;

public interface IQuaternionInterpolatable : IAxesInterpolatable<Quaternion>;

public interface ICombinedQuaternionKeyframes<TKeyframe>
    : IQuaternionInterpolatable, ICombinedAxesKeyframes<TKeyframe, Quaternion>
    where TKeyframe : IKeyframe<Quaternion>;

public interface ISeparateQuaternionKeyframes<TKeyframe>
    : IQuaternionInterpolatable, ISeparateAxesKeyframes<TKeyframe, float, Quaternion>
    where TKeyframe : IKeyframe<float>;

public interface ISeparateEulerRadiansKeyframes<TKeyframe>
    : IQuaternionInterpolatable,
      ISeparateAxesKeyframes<TKeyframe, float, Quaternion>
    where TKeyframe : IKeyframe<float> {
  // TODO: Slow! Switch to using generics/structs for a speedup here
  ConvertRadiansToQuaternion ConvertRadiansToQuaternionImpl { get; set; }

  delegate Quaternion ConvertRadiansToQuaternion(float xRadians,
                                                 float yRadians,
                                                 float zRadians);

  IRadiansKeyframeInterpolator<TKeyframe> Interpolator { get; }
}