using System;
using System.Runtime.CompilerServices;

namespace fin.animation.keyframes;

public readonly record struct KeyframeDefinition<T>(
    int Frame,
    T Value,
    string FrameType = "")
    : IComparable<KeyframeDefinition<T>>, IEquatable<KeyframeDefinition<T>> {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(KeyframeDefinition<T> other)
    => this.Frame - other.Frame;

  public bool Equals(KeyframeDefinition<T> other)
    => this.Frame == other.Frame &&
       (this.Value?.Equals(other.Value) ??
        this.Value == null && other.Value == null);
}