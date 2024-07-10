using System;
using System.Runtime.CompilerServices;

namespace fin.animation.keyframes;

public readonly record struct Keyframe<T>(
    float Frame,
    T ValueIn,
    T ValueOut)
    : IKeyframe<T>,
      IComparable<Keyframe<T>> {
  public Keyframe(float frame, T value) : this(frame, value, value) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(Keyframe<T> other)
    => FrameComparisonUtil.CompareFrames(this.Frame, other.Frame);

  public bool Equals(Keyframe<T> other)
    => FrameComparisonUtil.AreSameFrame(this.Frame, other.Frame) &&
       (this.ValueIn?.Equals(other.ValueIn) ??
        this.ValueIn == null && other.ValueIn == null) &&
       (this.ValueOut?.Equals(other.ValueOut) ??
        this.ValueOut == null && other.ValueOut == null);
}