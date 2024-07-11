using System.Numerics;

using fin.animation.keyframes;

namespace fin.animation.types.quaternion;

public interface IQuaternionInterpolatable : IAxesInterpolatable<Quaternion>;

public interface ICombinedQuaternionKeyframes<TKeyframe>
    : IQuaternionInterpolatable, ICombinedAxesKeyframes<TKeyframe, Quaternion>
    where TKeyframe : IKeyframe<Quaternion>;

public interface ISeparateQuaternionKeyframes<TKeyframe>
    : IQuaternionInterpolatable, ISeparateAxesKeyframes<TKeyframe, float, Quaternion>
    where TKeyframe : IKeyframe<float>;

public interface ISeparateEulerRadiansKeyframes<TKeyframe>
    : IQuaternionInterpolatable, ISeparateAxesKeyframes<TKeyframe, float, Quaternion>
    where TKeyframe : IKeyframe<float>;