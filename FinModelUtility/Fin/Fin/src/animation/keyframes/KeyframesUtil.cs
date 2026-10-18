using System;
using System.Collections.Generic;

using fin.animation.interpolation;
using fin.math;
using fin.math.floats;

namespace fin.animation.keyframes;

public static class KeyframesUtil {
  public static void AddKeyframe<TKeyframe>(
      this List<TKeyframe> impl,
      TKeyframe keyframe) where TKeyframe : IKeyframe {
    if (impl.Count == 0 || keyframe.Frame > impl[^1].Frame) {
      impl.Add(keyframe);
      return;
    }

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
      this ReadOnlySpan<TKeyframe> keyframes,
      float frame,
      ISharedInterpolationConfig sharedConfig,
      IIndividualInterpolationConfig individualConfig,
      out TKeyframe keyframe,
      out int keyframeIndex,
      out float normalizedFrame) where TKeyframe : IKeyframe {
    // Short-circuits early if there are no frames.
    if (keyframes.Length == 0) {
      keyframe = default;
      keyframeIndex = 0;
      normalizedFrame = 0;
      return false;
    }

    var looping = sharedConfig.Looping;

    if (looping) {
      frame = frame.ModRange(0,
                             individualConfig.AnimationLength ??
                             sharedConfig.AnimationLength);
    }

    var index = BinarySearchForPrecedingKeyframe_(keyframes, frame);

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
      index = keyframes.Length - 1;
    }

    keyframe = keyframes[index];
    keyframeIndex = index;
    normalizedFrame = frame;
    return true;
  }

  private static int BinarySearchForPrecedingKeyframe_<TKeyframe>(
      ReadOnlySpan<TKeyframe> keyframes,
      float frame)
      where TKeyframe : IKeyframe {
    int lowI = 0;
    int highI = keyframes.Length - 1;

    int lastLower = -1;

    unsafe {
      fixed (TKeyframe* ptr = &keyframes[0]) {
        while (lowI <= highI) {
          var midI = lowI + ((highI - lowI) >> 1);
          var midFrame = ptr[midI].Frame;

          if (midFrame == frame) {
            return midI;
          }

          // 2. Direct comparisons narrow down bounds cleanly
          if (midFrame < frame) {
            lastLower = midI;
            lowI = midI + 1;
          } else {
            highI = midI - 1;
          }
        }
      }
    }

    return lastLower;
  }

  public enum InterpolationDataType {
    NONE,
    PRECEDING_AND_FOLLOWING,
    PRECEDING_ONLY,
  }

  public static InterpolationDataType
      TryGetPrecedingAndFollowingKeyframesWithBinarySearch<
          TKeyframe>(
          this ReadOnlySpan<TKeyframe> keyframes,
          float frame,
          ISharedInterpolationConfig sharedConfig,
          IIndividualInterpolationConfig individualConfig,
          out TKeyframe precedingKeyframe,
          out TKeyframe followingKeyframe,
          out float normalizedFrame) where TKeyframe : IKeyframe {
    if (!keyframes.TryGetPrecedingKeyframe(frame,
                                           sharedConfig,
                                           individualConfig,
                                           out precedingKeyframe,
                                           out var precedingKeyframeIndex,
                                           out normalizedFrame)) {
      if (sharedConfig.Looping) {
        if (keyframes.Length == 1) {
          precedingKeyframe = keyframes[0];
          followingKeyframe = default!;
          return InterpolationDataType.PRECEDING_ONLY;
        }

        if (keyframes.Length > 1) {
          precedingKeyframe = keyframes[^1];
          followingKeyframe = keyframes[0];
          return InterpolationDataType.PRECEDING_AND_FOLLOWING;
        }
      }

      followingKeyframe = default!;
      return InterpolationDataType.NONE;
    }

    var followingKeyframeIndex = precedingKeyframeIndex + 1;
    if (followingKeyframeIndex == keyframes.Length) {
      // TODO: Should this support from a frame to itself?
      if (!sharedConfig.Looping || keyframes.Length == 1) {
        followingKeyframe = default!;
        return InterpolationDataType.PRECEDING_ONLY;
      }

      followingKeyframeIndex = 0;
    }

    followingKeyframe = keyframes[followingKeyframeIndex];
    return InterpolationDataType.PRECEDING_AND_FOLLOWING;
  }

  public static bool TryToInterpolateAtFrame
      <T, TKeyframe, TKeyframeInterpolator>(
          ISharedInterpolationConfig sharedConfig,
          IIndividualInterpolationConfig<T> individualConfig,
          TKeyframe precedingKeyframe,
          TKeyframe followingKeyframe,
          float normalizedFrame,
          TKeyframeInterpolator interpolator,
          InterpolationDataType interpolationDataType,
          out T value)
      where TKeyframe : IKeyframe<T>
      where TKeyframeInterpolator : IKeyframeInterpolator<TKeyframe, T> {
    switch (interpolationDataType) {
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

        value = default!;
        return false;
    }
  }
}