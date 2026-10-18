using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector3;

public sealed class SeparateVector3AdvancingInterpolator<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IndividualInterpolationConfig<float>? individualConfigX,
    IndividualInterpolationConfig<float>? individualConfigY,
    IndividualInterpolationConfig<float>? individualConfigZ,
    IReadOnlyList<TKeyframe> keyframesX,
    IReadOnlyList<TKeyframe> keyframesY,
    IReadOnlyList<TKeyframe> keyframesZ,
    IKeyframeInterpolator<TKeyframe, float> keyframeInterpolator)
    : IAdvancingInterpolator<Vector3>
    where TKeyframe : class, IKeyframe<float> {
  private readonly AdvancingInterpolator<float, TKeyframe> xImpl_
      = new(sharedConfig, individualConfigX, keyframesX, keyframeInterpolator);

  private readonly AdvancingInterpolator<float, TKeyframe> yImpl_
      = new(sharedConfig, individualConfigY, keyframesY, keyframeInterpolator);

  private readonly AdvancingInterpolator<float, TKeyframe> zImpl_
      = new(sharedConfig, individualConfigZ, keyframesZ, keyframeInterpolator);

  public bool TryGetAtFrame(float frame, out Vector3 value) {
    value = default;

    if (!this.xImpl_.TryGetAtFrame(frame, out var x)) {
      return false;
    }

    if (!this.yImpl_.TryGetAtFrame(frame, out var y)) {
      return false;
    }

    if (!this.zImpl_.TryGetAtFrame(frame, out var z)) {
      return false;
    }

    value = new Vector3(x, y, z);
    return true;  }
}