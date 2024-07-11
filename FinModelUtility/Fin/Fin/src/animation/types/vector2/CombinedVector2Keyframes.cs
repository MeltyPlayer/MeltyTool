using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector2;

public class CombinedVector2Keyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, Vector2> interpolator,
    IndividualInterpolationConfig<Vector2> individualConfig = default)
    : ICombinedVector2Keyframes<TKeyframe>
    where TKeyframe : IKeyframe<Vector2> {
  private readonly InterpolatedKeyframes<TKeyframe, Vector2> impl_
      = new(sharedConfig, interpolator, individualConfig);

  public ISharedInterpolationConfig SharedConfig => sharedConfig;

  public IndividualInterpolationConfig<Vector2> IndividualConfig
    => individualConfig;

  public IReadOnlyList<TKeyframe> Definitions => this.impl_.Definitions;
  public bool HasAnyData => this.impl_.HasAnyData;

  public void Add(TKeyframe keyframe) => this.impl_.Add(keyframe);

  public bool TryGetAtFrame(float frame, out Vector2 value)
    => this.impl_.TryGetAtFrame(frame, out value);
}