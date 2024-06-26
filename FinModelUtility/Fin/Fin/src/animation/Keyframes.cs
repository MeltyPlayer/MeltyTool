using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using fin.math;


namespace fin.animation {
  public readonly record struct Keyframe<T>(
      int Frame,
      T Value,
      string FrameType = "")
      : IComparable<Keyframe<T>>, IEquatable<Keyframe<T>> {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Keyframe<T> other)
      => this.Frame - other.Frame;

    public bool Equals(Keyframe<T> other)
      => this.Frame == other.Frame &&
         (this.Value?.Equals(other.Value) ??
          this.Value == null && other.Value == null);
  }

  public class Keyframes<T> : IKeyframes<T> {
    // List used to store the specific keyframe at each index. Wasteful
    // memory-wise, but allows us to have O(1) frame lookups in terms of time.
    private List<int> frameToKeyframe_;
    private List<Keyframe<T>> impl_;

    public Keyframes(int initialCapacity = 0) {
      this.impl_ = new(initialCapacity);
      this.frameToKeyframe_ = new List<int>(initialCapacity);
    }

    public IReadOnlyList<Keyframe<T>> Definitions => this.impl_;

    public bool HasAtLeastOneKeyframe => this.impl_.Count > 0;
    public int MaxKeyframe => Math.Max(this.frameToKeyframe_.Count - 1, 0);

    public void SetKeyframe(int frame, T value, string frameType = "") {
      var keyframeExists = this.FindIndexOfKeyframe(frame,
                                                    out var keyframeIndex,
                                                    out var existingKeyframe,
                                                    out var isLastKeyframe);

      var newKeyframe = new Keyframe<T>(frame, value, frameType);

      if (keyframeExists && existingKeyframe.Frame == frame) {
        this.impl_[keyframeIndex] = newKeyframe;
      } else if (keyframeExists && isLastKeyframe) {
        this.impl_.Add(newKeyframe);
      } else if (keyframeExists && existingKeyframe.Frame < frame) {
        this.impl_.Insert(keyframeIndex + 1, newKeyframe);
      } else {
        this.impl_.Insert(keyframeIndex, newKeyframe);
      }

      while (this.frameToKeyframe_.Count < frame + 1) {
        this.frameToKeyframe_.Add(0);
      }

      var currentFrame = this.MaxKeyframe;
      for (var k = this.impl_.Count - 1; k >= 0; --k) {
        var keyframe = this.impl_[k];

        while (keyframe.Frame <= currentFrame) {
          this.frameToKeyframe_[currentFrame--] = k;
        }
      }
    }

    public void SetAllKeyframes(IEnumerable<T> values) {
      this.impl_ = values
                   .Select((value, frame) => new Keyframe<T>(frame, value))
                   .ToList();

      var lastFrame = this.impl_.Last().Frame;
      while (this.frameToKeyframe_.Count < lastFrame + 1) {
        this.frameToKeyframe_.Add(0);
      }

      var currentFrame = this.MaxKeyframe;
      for (var k = this.impl_.Count - 1; k >= 0; --k) {
        var keyframe = this.impl_[k];

        while (keyframe.Frame <= currentFrame) {
          this.frameToKeyframe_[currentFrame--] = k;
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Keyframe<T> GetKeyframeAtIndex(int index) => this.impl_[index];

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
      var keyframe = this.GetKeyframeAtFrame(frame);
      if (keyframe.HasValue && keyframe.Value.Frame == frame) {
        return keyframe;
      }

      return null;
    }


    public bool FindIndexOfKeyframe(
        int frame,
        out int keyframeIndex,
        out Keyframe<T> keyframe,
        out bool isLastKeyframe) {
      if (this.frameToKeyframe_.Count == 0 || frame < 0) {
        keyframeIndex = 0;
        keyframe = default;
        isLastKeyframe = this.frameToKeyframe_.Count == 1;
        return false;
      }

      var maxKeyframe = this.MaxKeyframe;
      frame = frame.Clamp(0, maxKeyframe);
      keyframeIndex = this.frameToKeyframe_[frame];
      keyframe = this.impl_[keyframeIndex];
      isLastKeyframe = keyframeIndex == this.impl_.Count - 1;
      return frame >= keyframe.Frame;
    }


    public override string ToString() {
      var definitions = this.Definitions;
      if (definitions.Count == 0) {
        return "Keyframes[0]";
      }

      if (definitions.Count == 1) {
        return $"Keyframes[1] @ {definitions[0].Frame}";
      }


      var firstFrame = definitions.First().Frame;
      var lastFrame = definitions.Last().Frame;

      return
          $"Keyframes[{this.Definitions.Count}] @ {firstFrame} to {lastFrame}";
    }
  }
}