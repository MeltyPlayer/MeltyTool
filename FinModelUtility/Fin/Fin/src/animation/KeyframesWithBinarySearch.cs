using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using CommunityToolkit.HighPerformance;

using fin.util.enumerables;


namespace fin.animation {
  public class KeyframesWithBinarySearch<T> : IKeyframes<T> {
    private List<Keyframe<T>> impl_;

    public KeyframesWithBinarySearch(int initialCapacity = 0) {
      this.impl_ = new(initialCapacity);
    }

    public IReadOnlyList<Keyframe<T>> Definitions => this.impl_;

    public bool HasAtLeastOneKeyframe { get; set; }
    public int MaxKeyframe
      => this.impl_.MaxOrDefault(keyframe => keyframe.Frame);

    public void SetKeyframe(int frame, T value, string frameType = "")
      => SetKeyframe(frame, value, out _, frameType);

    public void SetKeyframe(int frame,
                            T value,
                            out bool performedBinarySearch,
                            string frameType = "") {
      this.HasAtLeastOneKeyframe = true;

      var keyframeExists = this.FindIndexOfKeyframe(frame,
                                                    out var keyframeIndex,
                                                    out var existingKeyframe,
                                                    out var isLastKeyframe,
                                                    out performedBinarySearch);

      var newKeyframe = new Keyframe<T>(frame, value, frameType);

      if (keyframeExists && existingKeyframe.Frame == frame) {
        this.lastAccessedKeyframeIndex_ = keyframeIndex;
        this.impl_[keyframeIndex] = newKeyframe;
      } else if (isLastKeyframe) {
        this.lastAccessedKeyframeIndex_ = this.impl_.Count;
        this.impl_.Add(newKeyframe);
      } else if (keyframeExists && existingKeyframe.Frame < frame) {
        this.impl_.Insert(keyframeIndex + 1, newKeyframe);
      } else {
        this.impl_.Insert(keyframeIndex, newKeyframe);
      }
    }

    public void SetAllKeyframes(IEnumerable<T> values) {
      this.impl_ = values
                   .Select((value, frame) => new Keyframe<T>(frame, value))
                   .ToList();
      this.HasAtLeastOneKeyframe = this.impl_.Count > 0;
      this.lastAccessedKeyframeIndex_ = this.impl_.Count - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Keyframe<T> GetKeyframeAtIndex(int index) 
      => this.impl_.AsSpan()[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Keyframe<T>? GetKeyframeAtFrame(int frame) {
      this.FindIndexOfKeyframe(frame,
                               out _,
                               out var keyframe,
                               out _);
      return keyframe;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Keyframe<T>? GetKeyframeAtExactFrame(int frame) {
      this.FindIndexOfKeyframe(frame,
                               out _,
                               out var keyframe,
                               out _);
      if (keyframe != null && keyframe.Frame == frame) {
        return keyframe;
      }

      return null;
    }


    private int lastAccessedKeyframeIndex_ = -1;

    public bool FindIndexOfKeyframe(
        int frame,
        out int keyframeIndex,
        out Keyframe<T> keyframe,
        out bool isLastKeyframe)
      => this.FindIndexOfKeyframe(frame,
                                  out keyframeIndex,
                                  out keyframe,
                                  out isLastKeyframe,
                                  out _);

    public bool FindIndexOfKeyframe(
        int frame,
        out int keyframeIndex,
        out Keyframe<T> keyframe,
        out bool isLastKeyframe,
        out bool performedBinarySearch) {
      performedBinarySearch = false;

      // Try to optimize the case where no frames have been processed yet.
      var keyframeCount = this.impl_.Count;
      if (this.lastAccessedKeyframeIndex_ == -1 || keyframeCount == 0) {
        this.lastAccessedKeyframeIndex_ = keyframeIndex = 0;
        keyframe = default;
        isLastKeyframe = false;
        return false;
      }

      var span = this.impl_.AsSpan();

      // Try to optimize the case where the next frame is being accessed.
      if (this.lastAccessedKeyframeIndex_ >= 0 &&
          this.lastAccessedKeyframeIndex_ < keyframeCount) {
        keyframeIndex = this.lastAccessedKeyframeIndex_;
        keyframe = span[keyframeIndex];

        if (frame >= keyframe.Frame) {
          isLastKeyframe = keyframeIndex == keyframeCount - 1;

          if (isLastKeyframe || frame == keyframe.Frame) {
            return true;
          }

          var nextKeyframe = span[this.lastAccessedKeyframeIndex_ + 1];
          if (nextKeyframe.Frame > frame) {
            return true;
          } else if (nextKeyframe.Frame == frame) {
            this.lastAccessedKeyframeIndex_ = ++keyframeIndex;
            keyframe = nextKeyframe;
            isLastKeyframe = keyframeIndex == keyframeCount - 1;
            return true;
          }
        }
      }

      // Perform a binary search for the current frame.
      var result = span.BinarySearch(new Keyframe<T>(frame, default!));
      performedBinarySearch = true;

      if (result >= 0) {
        this.lastAccessedKeyframeIndex_ = keyframeIndex = result;
        keyframe = this.impl_[keyframeIndex];
        isLastKeyframe = keyframeIndex == keyframeCount - 1;
        return true;
      }

      var i = ~result;
      if (i == keyframeCount) {
        this.lastAccessedKeyframeIndex_ = keyframeIndex = keyframeCount - 1;
        isLastKeyframe = true;
        if (keyframeCount > 0) {
          keyframe = span[keyframeIndex];
          return true;
        }

        keyframe = default;
        return false;
      }

      this.lastAccessedKeyframeIndex_ = keyframeIndex = Math.Max(0, i - 1);
      keyframe = span[keyframeIndex];
      var keyframeExists = keyframe.Frame <= frame;
      if (!keyframeExists) {
        keyframe = default;
      }

      isLastKeyframe = false;
      return keyframeExists;
    }
  }
}