using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector3;

public interface IVector3Interpolatable : IInterpolatable<Vector3>;

public interface ISeparateVector3Keyframes<TKeyframe>
    : IVector3Interpolatable, ISeparateAxesKeyframes<TKeyframe, float, Vector3>
    where TKeyframe : IKeyframe<float>;

public interface IPopulatedVector3Keyframes<TKeyframe>
    : IVector3Interpolatable, IPopulatedAxesKeyframes<TKeyframe, Vector3>
    where TKeyframe : IKeyframe<Vector3>;