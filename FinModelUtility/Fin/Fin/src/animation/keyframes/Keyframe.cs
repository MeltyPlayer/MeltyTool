using System;
using System.Runtime.CompilerServices;

namespace fin.animation.keyframes;

public readonly record struct Keyframe<T>(
    float Frame,
    T Value,
    string FrameType = "")
    : IKeyframe<T>,
      IComparable<Keyframe<T>> {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(Keyframe<T> other)
    => FrameComparisonUtil.CompareFrames(this.Frame, other.Frame);

  public bool Equals(KeyframeDefinition<T> other)
    => FrameComparisonUtil.AreSameFrame(this.Frame, other.Frame) &&
       (this.Value?.Equals(other.Value) ??
        this.Value == null && other.Value == null);
}