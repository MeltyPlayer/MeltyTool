using System.Collections.Generic;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types;

public interface IAxesInterpolatable<TAxes> : IInterpolatable<TAxes>;

public interface ISeparateAxesKeyframes<TAxisKeyframe, TAxis, TAxes>
    : IAxesInterpolatable<TAxes>
    where TAxisKeyframe : IKeyframe<TAxis> {
  IReadOnlyList<IInterpolatableKeyframes<TAxisKeyframe, TAxis>> Axes { get; }
}

public interface IPopulatedAxesKeyframes<TAxesKeyframe, TAxes>
    : IAxesInterpolatable<TAxes>, IKeyframes<TAxesKeyframe>
    where TAxesKeyframe : IKeyframe<TAxes>;