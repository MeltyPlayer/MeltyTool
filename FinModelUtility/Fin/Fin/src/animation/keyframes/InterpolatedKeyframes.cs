using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

using fin.animation.interpolation;

using static fin.animation.keyframes.KeyframesUtil;

namespace fin.animation.keyframes;

public interface IInterpolatableKeyframes<TKeyframe, T>
    : IKeyframes<TKeyframe>,
      IConfiguredInterpolatable<T> where TKeyframe : IKeyframe<T>;

public sealed class InterpolatedKeyframes<TKeyframe, T>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, T> interpolator,
    IndividualInterpolationConfig<T>? individualConfig = null)
    : IInterpolatableKeyframes<TKeyframe, T>
    where TKeyframe : IKeyframe<T> {
  private readonly List<TKeyframe> impl_
      = new(individualConfig?.InitialCapacity ?? 0);

  public ISharedInterpolationConfig SharedConfig => sharedConfig;

  public IndividualInterpolationConfig<T> IndividualConfig
    => individualConfig ?? IndividualInterpolationConfig<T>.DEFAULT;

  public IReadOnlyList<TKeyframe> Definitions => this.impl_;
  public bool HasAnyData => this.Definitions.Count > 0;

  public int AnimationLength => sharedConfig.AnimationLength;
  public bool Looping => sharedConfig.Looping;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Add(TKeyframe keyframe) => this.impl_.AddKeyframe(keyframe);

  public bool TryGetAtFrameWithBinarySearch(float frame, out T value) {
    var interpolationDataType
        = this.impl_.AsSpan()
              .TryGetPrecedingAndFollowingKeyframesWithBinarySearch(
                  frame,
                  sharedConfig,
                  this.IndividualConfig,
                  out var precedingKeyframe,
                  out var followingKeyframe,
                  out var normalizedFrame);
    return TryToInterpolateAtFrame(
        sharedConfig,
        this.IndividualConfig,
        precedingKeyframe,
        followingKeyframe,
        normalizedFrame,
        interpolator,
        interpolationDataType,
        out value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public InterpolationDataType TryGetPrecedingAndFollowingKeyframes(
      float frame,
      out TKeyframe precedingKeyframe,
      out TKeyframe followingKeyframe,
      out float normalizedFrame)
    => this.impl_.AsSpan()
           .TryGetPrecedingAndFollowingKeyframesWithBinarySearch(
               frame,
               sharedConfig,
               this.IndividualConfig,
               out precedingKeyframe,
               out followingKeyframe,
               out normalizedFrame);

  public void GetAllFrames(Span<T> dst) {
    T defaultValue = default!;
    this.IndividualConfig.DefaultValue?.Try(out defaultValue);
    dst.Fill(defaultValue);
    if (this.impl_.Count == 0) {
      return;
    }

    var from = this.impl_[0];
    if (!sharedConfig.Looping) {
      dst[(int) MathF.Ceiling(from.Frame)..].Fill(from.ValueOut);
    }

    for (var k = 1; k < this.impl_.Count; ++k) {
      var to = this.impl_[k];
      this.AddFrames_(dst, from, to);
      from = to;
    }

    if (sharedConfig.Looping) {
      this.AddFrames_(dst, this.impl_[^1], this.impl_[0]);
    } else {
      var last = this.impl_[^1];
      var lastFrame = (int) MathF.Ceiling(last.Frame);
      if (lastFrame < dst.Length) {
        dst[lastFrame..].Fill(last.ValueOut);
      }
    }
  }

  private void AddFrames_(Span<T> dst, TKeyframe from, TKeyframe to) {
    var fromFrame = (int) MathF.Ceiling(from.Frame);
    var toFrame = (int) MathF.Ceiling(to.Frame);

    if (toFrame < fromFrame) {
      toFrame += dst.Length;
    }

    for (var i = fromFrame; i < toFrame; ++i) {
      var normalizedFrame = i % dst.Length;
      dst[normalizedFrame]
          = interpolator.Interpolate(from, to, normalizedFrame, sharedConfig);
    }
  }

  public bool TryGetSimpleKeyframes(
      out IReadOnlyList<(float frame, T value)> keyframes,
      out IReadOnlyList<(T tangentIn, T tangentOut)>? tangentKeyframes) {
    // TODO: Implement this
    if (this.impl_ is List<KeyframeWithTangents<T>>) {
      keyframes = null;
      tangentKeyframes = null;
      return false;
    }

    tangentKeyframes = null;
    keyframes = this.impl_.Select(k => (k.Frame, k.ValueOut)).ToArray();

    return true;
  }
}