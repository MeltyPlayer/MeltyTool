using System.Collections.Generic;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types;

public interface IAxesInterpolatable<TAxes> : IInterpolatable<TAxes>;

public interface ICombinedAxesKeyframes<TAxesKeyframe, TAxes>
    : IAxesInterpolatable<TAxes>, IKeyframes<TAxesKeyframe>
    where TAxesKeyframe : IKeyframe<TAxes>;

public interface ISeparateAxesKeyframes<TAxisKeyframe, TAxis, TAxes>
    : IAxesInterpolatable<TAxes>
    where TAxisKeyframe : IKeyframe<TAxis> {
  IReadOnlyList<IInterpolatableKeyframes<TAxisKeyframe, TAxis>> Axes { get; }

  bool IInterpolatable<TAxes>.HasAnyData
    => this.Axes[0].HasAnyData ||
       this.Axes[1].HasAnyData ||
       this.Axes[2].HasAnyData;
}