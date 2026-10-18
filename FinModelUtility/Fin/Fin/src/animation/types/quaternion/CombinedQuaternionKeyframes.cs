using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.quaternion;

public sealed class CombinedQuaternionKeyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, Quaternion> interpolator,
    IndividualInterpolationConfig<Quaternion>? individualConfig = null)
    : ICombinedQuaternionKeyframes<TKeyframe>
    where TKeyframe : IKeyframe<Quaternion> {
  private readonly InterpolatedKeyframes<TKeyframe, Quaternion> impl_
      = new(sharedConfig, interpolator, individualConfig);

  public ISharedInterpolationConfig SharedConfig => sharedConfig;

  public IndividualInterpolationConfig<Quaternion>? IndividualConfig
    => individualConfig;

  public IReadOnlyList<TKeyframe> Definitions => this.impl_.Definitions;

  public bool HasAnyData => this.impl_.HasAnyData;

  public int AnimationLength => sharedConfig.AnimationLength;
  public bool Looping => sharedConfig.Looping;

  public void Add(TKeyframe keyframe) => this.impl_.Add(keyframe);

  public bool TryGetAtFrameWithBinarySearch(float frame, out Quaternion value)
    => this.impl_.TryGetAtFrameWithBinarySearch(frame, out value);

  public void GetAllFrames(Span<Quaternion> dst) => this.impl_.GetAllFrames(dst);

  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, Quaternion value)> keyframes,
      out IReadOnlyList<(Quaternion tangentIn, Quaternion tangentOut)>? tangentKeyframes)
    => this.impl_.TryGetSimpleKeyframes(out keyframes, out tangentKeyframes);
}