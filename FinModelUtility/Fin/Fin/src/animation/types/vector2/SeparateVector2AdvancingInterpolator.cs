using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector2;

public sealed class SeparateVector2AdvancingInterpolator<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IndividualInterpolationConfig<float>? individualConfigX,
    IndividualInterpolationConfig<float>? individualConfigY,
    IReadOnlyList<TKeyframe> keyframesX,
    IReadOnlyList<TKeyframe> keyframesY,
    IKeyframeInterpolator<TKeyframe, float> keyframeInterpolator)
    : IAdvancingInterpolator<Vector2>
    where TKeyframe : class, IKeyframe<float> {
  private readonly AdvancingInterpolator<float, TKeyframe> xImpl_
      = new(sharedConfig, individualConfigX, keyframesX, keyframeInterpolator);

  private readonly AdvancingInterpolator<float, TKeyframe> yImpl_
      = new(sharedConfig, individualConfigY, keyframesY, keyframeInterpolator);

  public bool TryGetAtFrame(float frame, out Vector2 value) {
    value = default;

    if (!this.xImpl_.TryGetAtFrame(frame, out var x)) {
      return false;
    }

    if (!this.yImpl_.TryGetAtFrame(frame, out var y)) {
      return false;
    }

    value = new Vector2(x, y);
    return true;  }
}