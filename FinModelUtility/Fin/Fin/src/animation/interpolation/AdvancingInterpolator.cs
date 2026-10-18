using System.Collections.Generic;

using fin.animation.keyframes;
using fin.math;

using static fin.animation.keyframes.KeyframesUtil;

namespace fin.animation.interpolation;

public interface IAdvancingInterpolator<T> {
  bool TryGetAtFrame(float frame, out T value);
}

public sealed class AdvancingInterpolator<T, TKeyframe>(
    ISharedInterpolationConfig sharedConfig,
    IIndividualInterpolationConfig<T>? individualConfig,
    IReadOnlyList<TKeyframe> keyframes,
    IKeyframeInterpolator<TKeyframe, T> keyframeInterpolator)
    : IAdvancingInterpolator<T>
    where TKeyframe : class, IKeyframe<T> {
  private int precedingKeyframeIndex_;

  private KeyframesUtil.InterpolationDataType interpolationDataType_;
  private TKeyframe precedingKeyframe_;
  private TKeyframe? followingKeyframe_;
  private float normalizedFrame_;

  public bool TryGetAtFrame(float frame, out T value) {
    this.UpdateKeyframeMetadata_(frame);

    return KeyframesUtil.TryToInterpolateBetweenKeyframes(
        sharedConfig,
        individualConfig,
        this.precedingKeyframe_,
        this.followingKeyframe_,
        this.normalizedFrame_,
        keyframeInterpolator,
        this.interpolationDataType_,
        out value);
  }

  private void UpdateKeyframeMetadata_(float frame) {
    if (!this.TryToGetPrecedingKeyframe_(
            frame,
            out this.precedingKeyframe_,
            out var precedingKeyframeIndex,
            out this.normalizedFrame_)) {
      if (sharedConfig.Looping) {
        // TODO: Should it be allowed to loop into itself?
        if (keyframes.Count == 1) {
          this.precedingKeyframe_ = keyframes[this.precedingKeyframeIndex_ = 0];
          this.followingKeyframe_ = null;
          this.interpolationDataType_ = InterpolationDataType.PRECEDING_ONLY;
          return;
        }

        if (keyframes.Count > 1) {
          this.precedingKeyframe_
              = keyframes[this.precedingKeyframeIndex_ = keyframes.Count - 1];
          this.followingKeyframe_ = keyframes[0];
          this.interpolationDataType_
              = InterpolationDataType.PRECEDING_AND_FOLLOWING;
          return;
        }
      }

      this.followingKeyframe_ = null;
      this.interpolationDataType_ = InterpolationDataType.NONE;
      return;
    }

    var followingKeyframeIndex = precedingKeyframeIndex + 1;
    if (followingKeyframeIndex == keyframes.Count) {
      // TODO: Should this support from a frame to itself?
      if (!sharedConfig.Looping || keyframes.Count == 1) {
        this.followingKeyframe_ = null;
        this.interpolationDataType_ = InterpolationDataType.PRECEDING_ONLY;
        return;
      }

      followingKeyframeIndex = 0;
    }

    this.followingKeyframe_ = keyframes[followingKeyframeIndex];
    this.interpolationDataType_ = InterpolationDataType.PRECEDING_AND_FOLLOWING;
  }

  private bool TryToGetPrecedingKeyframe_(
      float frame,
      out TKeyframe keyframe,
      out int keyframeIndex,
      out float normalizedFrame) {
    // Short-circuits early if there are no frames.
    if (keyframes.Count == 0) {
      keyframe = default;
      keyframeIndex = 0;
      normalizedFrame = 0;
      return false;
    }

    var looping = sharedConfig.Looping;
    if (looping) {
      frame = frame.ModRange(
          0,
          individualConfig.AnimationLength ??
          sharedConfig.AnimationLength);
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
      index = keyframes.Count - 1;
      keyframe = keyframes[index];
    }

    keyframeIndex = index;
    normalizedFrame = frame;
    return true;
  }

  private int SearchForPrecedingKeyframe_(
      float frame,
      out TKeyframe precedingKeyframe) {
    // If still in current keyframe pair, just leave it as-is.
    if (this.followingKeyframe_ != null &&
        this.followingKeyframe_.Frame > frame &&
        this.precedingKeyframe_.Frame <= frame) {
      precedingKeyframe = this.precedingKeyframe_;
      return this.precedingKeyframeIndex_;
    }

    var looping = sharedConfig.Looping;

    var keyframeI = this.precedingKeyframeIndex_;

    // Oops, we looped. Let's start at 0 instead.
    if (frame < this.precedingKeyframe_.Frame) {
      keyframeI = 0;
    }

    // We'll loop through each keyframe. This should usually only have to check
    // the next one.
    var lastPreviousIndex = -1;
    TKeyframe lastPreviousKeyframe = null;

    for (var i = 0; i < keyframes.Count; ++i) {
      // If we passed the end...
      if (keyframeI >= keyframes.Count) {
        // If this is not looping, break early.
        if (!looping) {
          precedingKeyframe = null;
          return -1;
        }

        keyframeI %= keyframes.Count;
      }

      var currentKeyframe = keyframes[keyframeI];
      if (currentKeyframe.Frame <= frame) {
        lastPreviousIndex = keyframeI;
        lastPreviousKeyframe = currentKeyframe;
      } else {
        break;
      }

      ++keyframeI;
    }

    precedingKeyframe = lastPreviousKeyframe;
    return lastPreviousIndex;
  }
}