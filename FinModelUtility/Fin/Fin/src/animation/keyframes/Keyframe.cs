using System;
using System.Runtime.CompilerServices;

namespace fin.animation.keyframes;

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