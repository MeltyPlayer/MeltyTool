using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using fin.animation.interpolation;
using fin.math;
using fin.util.lists;

namespace fin.animation.keyframes;

public static class KeyframesUtil {
  public static void AddKeyframe<TKeyframe>(
      this List<TKeyframe> impl,
      TKeyframe keyframe) where TKeyframe : IKeyframe {
    var index = impl.BinarySearch(keyframe);

    // If index is greater than 0, then we found an exact match! We can replace
    // that specific keyframe.
    if (index >= 0) {
      impl[index] = keyframe;
      return;
    }

    // Otherwise, the exact value wasn't found in the list. The index is
    // instead the bitwise complement of the next element that's larger.
    impl.Insert(~index, keyframe);
  }

  public static bool TryGetPrecedingKeyframe<TKeyframe>(
      this List<TKeyframe> impl,
      float frame,
      ISharedInterpolationConfig sharedConfig,
      out TKeyframe keyframe,
      out int keyframeIndex) where TKeyframe : IKeyframe {
    // Short-circuits early if there are no frames.
    if (impl.Count == 0) {
      keyframe = default;
      keyframeIndex = default;
      return false;
    }

    var looping = sharedConfig.Looping;

    if (looping) {
      frame = frame.ModRange(0, sharedConfig.AnimationLength);
    }

    var index
        = impl.BinarySearch<TKeyframe, float, KeyframeFrameComparer<TKeyframe>>(
            new KeyframeFrameComparer<TKeyframe>(frame));

    // If index is negative then the exact value wasn't found in the list. It
    // will instead be the bitwise complement of the next element that's larger.
    if (index < 0) {
      index = ~index;

      // Subtract one because we want to find the previous frame.
      index -= 1;

      // If index is negative after subtracting, then no frames are smaller.
      if (index < 0) {
        // If not looping, short-circuits early.
        if (!looping) {
          keyframe = default;
          keyframeIndex = default;
          return false;
        }

        // Otherwise, wraps back around to the last frame.
        index = impl.Count - 1;
      }
    }

    keyframe = impl[index];
    keyframeIndex = index;
    return true;
  }

  private readonly struct KeyframeFrameComparer<TKeyframe>(float frame)
      : IStaticAsymmetricComparer<TKeyframe, float>
      where TKeyframe : IKeyframe {
    public float Value => frame;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(TKeyframe lhs, float rhs)
      => FrameComparisonUtil.CompareFrames(lhs.Frame, rhs);
  }

  public enum InterpolationDataType {
    NONE,
    PRECEDING_AND_FOLLOWING,
    PRECEDING_ONLY,
  }

  public static InterpolationDataType TryGetPrecedingAndFollowingKeyframes<
      TKeyframe>(
      this List<TKeyframe> impl,
      float frame,
      ISharedInterpolationConfig sharedConfig,
      out TKeyframe precedingKeyframe,
      out TKeyframe followingKeyframe) where TKeyframe : IKeyframe {
    if (!impl.TryGetPrecedingKeyframe(frame,
                                      sharedConfig,
                                      out precedingKeyframe,
                                      out var precedingKeyframeIndex)) {
      followingKeyframe = default;
      return InterpolationDataType.NONE;
    }

    var followingKeyframeIndex = precedingKeyframeIndex + 1;
    if (followingKeyframeIndex == impl.Count) {
      if (!sharedConfig.Looping) {
        followingKeyframe = default;
        return InterpolationDataType.PRECEDING_ONLY;
      }

      followingKeyframeIndex = 0;
    }

    followingKeyframe = impl[followingKeyframeIndex];
    return InterpolationDataType.PRECEDING_AND_FOLLOWING;
  }
}