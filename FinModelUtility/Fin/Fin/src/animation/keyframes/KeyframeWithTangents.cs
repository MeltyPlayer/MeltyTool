using System;
using System.Runtime.CompilerServices;

namespace fin.animation.keyframes;

public readonly record struct KeyframeWithTangents<T>(
    float Frame,
    T Value,
    float TangentIn,
    float TangentOut)
    : IKeyframeWithTangents<T>, IComparable<KeyframeWithTangents<T>> {
  public KeyframeWithTangents(float frame, T value, float tangent) : this(
      frame,
      value,
      tangent,
      tangent) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(KeyframeWithTangents<T> other)
    => FrameComparisonUtil.CompareFrames(this.Frame, other.Frame);

  public bool Equals(KeyframeWithTangents<T> other)
    => FrameComparisonUtil.AreSameFrame(this.Frame, other.Frame) &&
       (this.Value?.Equals(other.Value) ??
        this.Value == null && other.Value == null);
}