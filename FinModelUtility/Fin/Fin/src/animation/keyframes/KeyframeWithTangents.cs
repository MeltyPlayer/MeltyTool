using System;
using System.Runtime.CompilerServices;

namespace fin.animation.keyframes;

public readonly record struct KeyframeWithTangents<T>(
    float Frame,
    T ValueIn,
    T ValueOut,
    float TangentIn,
    float TangentOut)
    : IKeyframeWithTangents<T>, IComparable<KeyframeWithTangents<T>> {
  public KeyframeWithTangents(float frame, T value, float tangent) : this(
      frame,
      value,
      value,
      tangent,
      tangent) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(KeyframeWithTangents<T> other)
    => FrameComparisonUtil.CompareFrames(this.Frame, other.Frame);

  public bool Equals(KeyframeWithTangents<T> other)
    => FrameComparisonUtil.AreSameFrame(this.Frame, other.Frame) &&
       (this.ValueIn?.Equals(other.ValueIn) ??
        this.ValueIn == null && other.ValueIn == null) &&
       (this.ValueOut?.Equals(other.ValueOut) ??
        this.ValueOut == null && other.ValueOut == null);
}