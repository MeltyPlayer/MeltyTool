using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.radians;

public interface IRadiansKeyframeInterpolator<in TKeyframe>
    : IKeyframeInterpolator<TKeyframe, float>
    where TKeyframe : IKeyframe<float>;