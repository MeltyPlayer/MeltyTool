using System.Numerics;

using fin.animation.keyframes;

namespace fin.animation.types.vector2;

public interface IVector2Interpolatable : IAxesInterpolatable<Vector2>;

public interface ICombinedVector2Keyframes<TKeyframe>
    : IVector2Interpolatable, ICombinedAxesKeyframes<TKeyframe, Vector2>
    where TKeyframe : IKeyframe<Vector2>;

public interface ISeparateVector2Keyframes<TKeyframe>
    : IVector2Interpolatable, ISeparateAxesKeyframes<TKeyframe, float, Vector2>
    where TKeyframe : IKeyframe<float>;