using System.Collections.Generic;
using System.Runtime.CompilerServices;

using fin.animation.keyframes;
using fin.math;

namespace fin.animation.interpolation;

public interface IAdvancingInterpolator<T> {
  bool TryGetAtFrame(float frame, out T value);
}

public sealed class AdvancingInterpolator<T, TKeyframe>
    : IAdvancingInterpolator<T>
    where TKeyframe : class, IKeyframe<T> {
  private readonly int animationLength_;
  private int keyframePairI_ = 0;
  private readonly (TKeyframe precedingKeyframe, int precedingKeyframeI, float fromFrame, float toFrame)[] keyframeTuples_;

  private KeyframesUtil.InterpolationDataType interpolationDataType_;
  private TKeyframe? precedingKeyframe_;
  private TKeyframe? followingKeyframe_;
  private float normalizedFrame_;
  private readonly ISharedInterpolationConfig sharedConfig_;
  private readonly IIndividualInterpolationConfig<T>? individualConfig_;
  private readonly IReadOnlyList<TKeyframe> keyframes_;
  private readonly IKeyframeInterpolator<TKeyframe, T> keyframeInterpolator_;

  public AdvancingInterpolator(ISharedInterpolationConfig sharedConfig,
                               IIndividualInterpolationConfig<T>?
                                   individualConfig,
                               IReadOnlyList<TKeyframe> keyframes,
                               IKeyframeInterpolator<TKeyframe, T>
                                   keyframeInterpolator) {
    this.animationLength_ = individualConfig?.AnimationLength ??
                            sharedConfig.AnimationLength;

    this.sharedConfig_ = sharedConfig;
    this.individualConfig_ = individualConfig;
    this.keyframes_ = keyframes;
    this.keyframeInterpolator_ = keyframeInterpolator;

    if (this.keyframes_.Count < 2) {
      this.keyframeTuples_ = [];
    } else {
      var looping = sharedConfig.Looping;

      var keyframeTuples = new List<(TKeyframe precedingKeyframe, int precedingKeyframeI, float fromFrame, float toFrame)>();

      var firstKeyframe = this.keyframes_[0];
      var lastKeyframe = this.keyframes_[^1];
      var lastKeyframeI = this.keyframes_.Count - 1;

      if (looping) {
        keyframeTuples.Add((lastKeyframe, lastKeyframeI, 0, firstKeyframe.Frame));
      }

      for (var i = 0; i < this.keyframes_.Count - 1; ++i) {
        var precedingKeyframe = this.keyframes_[i];
        var followingKeyframe = this.keyframes_[i + 1];

        keyframeTuples.Add((precedingKeyframe, i, precedingKeyframe.Frame, followingKeyframe.Frame));
      }

      if (looping) {
        keyframeTuples.Add((lastKeyframe, lastKeyframeI, lastKeyframe.Frame, this.animationLength_));
      }

      this.keyframeTuples_ = keyframeTuples.ToArray();
    }
  }

  public bool TryGetAtFrame(float frame, out T value) {
    this.UpdateKeyframeMetadata_(frame);

    return KeyframesUtil.TryToInterpolateBetweenKeyframes(
        this.sharedConfig_,
        this.individualConfig_,
        this.precedingKeyframe_,
        this.followingKeyframe_,
        this.normalizedFrame_,
        this.keyframeInterpolator_,
        this.interpolationDataType_,
        out value);
  }

  public KeyframesUtil.InterpolationDataType TryGetPrecedingAndFollowingKeyframes(
      float frame,
      out TKeyframe precedingKeyframe,
      out TKeyframe followingKeyframe) {
    this.UpdateKeyframeMetadata_(frame);
    precedingKeyframe = this.precedingKeyframe_;
    followingKeyframe = this.followingKeyframe_;
    return this.interpolationDataType_;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void UpdateKeyframeMetadata_(float frame) {
    if (!this.TryToGetPrecedingKeyframe_(
            frame,
            out this.precedingKeyframe_,
            out var precedingKeyframeIndex,
            out this.normalizedFrame_)) {
      if (this.sharedConfig_.Looping) {
        // TODO: Should it be allowed to loop into itself?
        if (this.keyframes_.Count == 1) {
          this.precedingKeyframe_ = this.keyframes_[0];
          this.followingKeyframe_ = null;
          this.interpolationDataType_ = KeyframesUtil.InterpolationDataType.PRECEDING_ONLY;
          return;
        }

        if (this.keyframes_.Count > 1) {
          this.precedingKeyframe_ = this.keyframes_[^1];
          this.followingKeyframe_ = this.keyframes_[0];
          this.interpolationDataType_
              = KeyframesUtil.InterpolationDataType.PRECEDING_AND_FOLLOWING;
          return;
        }
      }

      this.followingKeyframe_ = null;
      this.interpolationDataType_ = KeyframesUtil.InterpolationDataType.NONE;
      return;
    }

    var followingKeyframeIndex = precedingKeyframeIndex + 1;
    if (followingKeyframeIndex == this.keyframes_.Count) {
      // TODO: Should this support from a frame to itself?
      if (!this.sharedConfig_.Looping || this.keyframes_.Count == 1) {
        this.followingKeyframe_ = null;
        this.interpolationDataType_ = KeyframesUtil.InterpolationDataType.PRECEDING_ONLY;
        return;
      }

      followingKeyframeIndex = 0;
    }

    this.followingKeyframe_ = this.keyframes_[followingKeyframeIndex];
    this.interpolationDataType_ = KeyframesUtil.InterpolationDataType.PRECEDING_AND_FOLLOWING;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private bool TryToGetPrecedingKeyframe_(
      float frame,
      out TKeyframe keyframe,
      out int keyframeIndex,
      out float normalizedFrame) {
    // Short-circuits early if there are no frames.
    if (this.keyframes_.Count == 0) {
      keyframe = default;
      keyframeIndex = 0;
      normalizedFrame = 0;
      return false;
    }

    var looping = this.sharedConfig_.Looping;
    if (looping) {
      frame = frame.Mod0ToMax(this.animationLength_);
    }

    var index = this.SearchForPrecedingKeyframe_(frame, out keyframe);

    // If index is -1, no frames are smaller.
    if (index == -1) {
      // If not looping, short-circuits early.
      if (!looping) {
        keyframe = default;
        keyframeIndex = 0;
        normalizedFrame = 0;
        return false;
      }

      // Otherwise, wraps back around to the last frame.
      index = this.keyframes_.Count - 1;
      keyframe = this.keyframes_[index];
    }

    keyframeIndex = index;
    normalizedFrame = frame;
    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int SearchForPrecedingKeyframe_(
      float frame,
      out TKeyframe precedingKeyframe) {
    var keyframePairI = this.keyframePairI_;

    // Oops, we looped. Let's start at 0 instead.
    if (this.precedingKeyframe_ != null &&
        frame < this.precedingKeyframe_.Frame) {
      keyframePairI = 0;
    }

    for (; keyframePairI < this.keyframeTuples_.Length; ++keyframePairI) {
      var keyframeTuple = this.keyframeTuples_[keyframePairI];

      if (keyframeTuple.fromFrame <= frame && keyframeTuple.toFrame > frame) {
        precedingKeyframe = keyframeTuple.precedingKeyframe;
        this.keyframePairI_ = keyframePairI;
        return keyframeTuple.precedingKeyframeI;
      }
    }

    // Hmm, didn't find any. Let's just give up.
    precedingKeyframe = null;
    return -1;
  }
}