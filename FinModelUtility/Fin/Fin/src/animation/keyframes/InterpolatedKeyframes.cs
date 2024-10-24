using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using fin.animation.interpolation;

using static fin.animation.keyframes.KeyframesUtil;

namespace fin.animation.keyframes;

public interface IInterpolatableKeyframes<TKeyframe, T>
    : IKeyframes<TKeyframe>,
      IConfiguredInterpolatable<T> where TKeyframe : IKeyframe<T>;

public class InterpolatedKeyframes<TKeyframe, T>(
    ISharedInterpolationConfig sharedConfig,
    IKeyframeInterpolator<TKeyframe, T> interpolator,
    IndividualInterpolationConfig<T> individualConfig = default)
    : IInterpolatableKeyframes<TKeyframe, T>
    where TKeyframe : IKeyframe<T> {
  private readonly List<TKeyframe>
      impl_ = new(individualConfig.InitialCapacity);

  public ISharedInterpolationConfig SharedConfig => sharedConfig;
  public IndividualInterpolationConfig<T> IndividualConfig => individualConfig;

  public IReadOnlyList<TKeyframe> Definitions => this.impl_;
  public bool HasAnyData => this.Definitions.Count > 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Add(TKeyframe keyframe) => this.impl_.AddKeyframe(keyframe);

  public bool TryGetAtFrame(float frame, out T value) {
    switch (this.impl_.TryGetPrecedingAndFollowingKeyframes(
                frame,
                sharedConfig,
                individualConfig,
                out var precedingKeyframe,
                out var followingKeyframe,
                out var normalizedFrame)) {
      case InterpolationDataType.PRECEDING_AND_FOLLOWING:
        value = interpolator.Interpolate(precedingKeyframe,
                                         followingKeyframe,
                                         normalizedFrame,
                                         sharedConfig);
        return true;

      case InterpolationDataType.PRECEDING_ONLY:
        value = precedingKeyframe.ValueOut;
        return true;

      default:
      case InterpolationDataType.NONE:
        if (individualConfig.DefaultValue?.Try(out value) ?? false) {
          return true;
        }

        value = default;
        return false;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public InterpolationDataType TryGetPrecedingAndFollowingKeyframes(
      float frame,
      out TKeyframe precedingKeyframe,
      out TKeyframe followingKeyframe,
      out float normalizedFrame)
    => this.impl_.TryGetPrecedingAndFollowingKeyframes(
        frame,
        sharedConfig,
        individualConfig,
        out precedingKeyframe,
        out followingKeyframe,
        out normalizedFrame);

  public void GetAllFrames(Span<T> dst) {
    T defaultValue = default!;
    individualConfig.DefaultValue?.Try(out defaultValue);
    dst.Fill(defaultValue);
    if (this.impl_.Count == 0) {
      return;
    }

    var from = this.impl_[0];
    if (!sharedConfig.Looping) {
      dst.Slice((int) MathF.Ceiling(from.Frame)).Fill(from.ValueOut);
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
        dst.Slice(lastFrame).Fill(last.ValueOut);
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
}