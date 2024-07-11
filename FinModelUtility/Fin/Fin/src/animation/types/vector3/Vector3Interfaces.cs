using System.Numerics;

using fin.animation.keyframes;

namespace fin.animation.types.vector3;

public interface IVector3Interpolatable : IAxesInterpolatable<Vector3>;

public interface ICombinedVector3Keyframes<TKeyframe>
    : IVector3Interpolatable, ICombinedAxesKeyframes<TKeyframe, Vector3>
    where TKeyframe : IKeyframe<Vector3>;

public interface ISeparateVector3Keyframes<TKeyframe>
    : IVector3Interpolatable, ISeparateAxesKeyframes<TKeyframe, float, Vector3>
    where TKeyframe : IKeyframe<float>;