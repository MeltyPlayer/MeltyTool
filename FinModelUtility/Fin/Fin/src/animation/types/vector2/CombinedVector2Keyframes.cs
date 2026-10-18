using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;

namespace fin.animation.types.vector2;

public sealed class CombinedVector2Keyframes<TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, Vector2> interpolator,
    IndividualInterpolationConfig<Vector2>? individualConfig = null)
    : ICombinedVector2Keyframes<TKeyframe>
    where TKeyframe : IKeyframe<Vector2> {
  private readonly InterpolatedKeyframes<TKeyframe, Vector2> impl_
      = new(sharedConfig, interpolator, individualConfig);

  public ISharedInterpolationConfig SharedConfig => sharedConfig;

  public IndividualInterpolationConfig<Vector2>? IndividualConfig
    => individualConfig;

  public IReadOnlyList<TKeyframe> Definitions => this.impl_.Definitions;
  public bool HasAnyData => this.impl_.HasAnyData;
  
  public int AnimationLength => sharedConfig.AnimationLength;
  public bool Looping => sharedConfig.Looping;

  public void Add(TKeyframe keyframe) => this.impl_.Add(keyframe);

  public bool TryGetAtFrameWithBinarySearch(float frame, out Vector2 value)
    => this.impl_.TryGetAtFrameWithBinarySearch(frame, out value);

  public void GetAllFrames(Span<Vector2> dst) => this.impl_.GetAllFrames(dst);

  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, Vector2 value)> keyframes,
      out IReadOnlyList<(Vector2 tangentIn, Vector2 tangentOut)>? tangentKeyframes)
    => this.impl_.TryGetSimpleKeyframes(out keyframes, out tangentKeyframes);
}