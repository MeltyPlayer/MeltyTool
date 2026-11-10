using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector3;

public sealed class SeparateQuaternionAdvancingInterpolator<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IndividualInterpolationConfig<float>? individualConfigX,
    IndividualInterpolationConfig<float>? individualConfigY,
    IndividualInterpolationConfig<float>? individualConfigZ,
    IndividualInterpolationConfig<float>? individualConfigW,
    IReadOnlyList<TKeyframe> keyframesX,
    IReadOnlyList<TKeyframe> keyframesY,
    IReadOnlyList<TKeyframe> keyframesZ,
    IReadOnlyList<TKeyframe> keyframesW,
    IKeyframeInterpolator<TKeyframe, float> keyframeInterpolator)
    : IAdvancingInterpolator<Quaternion>
    where TKeyframe : class, IKeyframe<float> {
  private readonly AdvancingInterpolator<float, TKeyframe> xImpl_
      = new(sharedConfig, individualConfigX, keyframesX, keyframeInterpolator);

  private readonly AdvancingInterpolator<float, TKeyframe> yImpl_
      = new(sharedConfig, individualConfigY, keyframesY, keyframeInterpolator);

  private readonly AdvancingInterpolator<float, TKeyframe> zImpl_
      = new(sharedConfig, individualConfigZ, keyframesZ, keyframeInterpolator);

  private readonly AdvancingInterpolator<float, TKeyframe> wImpl_
      = new(sharedConfig, individualConfigW, keyframesW, keyframeInterpolator);

  public bool TryGetAtFrame(float frame, out Quaternion value) {
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

    if (!this.wImpl_.TryGetAtFrame(frame, out var w)) {
      return false;
    }

    value = new Quaternion(x, y, z, w);
    return true;
  }
}