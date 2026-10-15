using System;
using System.Runtime.CompilerServices;

using CommunityToolkit.Diagnostics;

namespace fin.util.spans;

public readonly unsafe ref struct WriteOnlySpan<T> where T : unmanaged {
  public T* Pointer { get; }
  public int Length { get; }

  public WriteOnlySpan(T* ptr, int length) {
    Guard.IsGreaterThan(length, 0);

    this.Pointer = ptr;
    this.Length = length;
  }

  public T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      Guard.IsInRange(index, 0, this.Length);
      unsafe {
        this.Pointer[index] = value;
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public WriteOnlySpan<T> Slice(int length) => this.Slice(0, length);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe WriteOnlySpan<T> Slice(int offset, int length) {
    Guard.IsInRange(offset, 0, this.Length);
    Guard.IsGreaterThan(length, 0);
    Guard.IsLessThanOrEqualTo(length, this.Length - offset);

    return new WriteOnlySpan<T>(this.Pointer + offset, length);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe WriteOnlySpan<TTo> Cast<TTo>() where TTo : unmanaged {
    var fromSize = sizeof(T);
    var toSize = sizeof(TTo);

    var sizeInBytes = this.Length * fromSize;
    var toLength = sizeInBytes / toSize;

    return new WriteOnlySpan<TTo>((TTo*) this.Pointer, toLength);
  }

  public static implicit operator WriteOnlySpan<T>(Span<T> span) {
    fixed (T* ptr = &span[0]) {
      return new WriteOnlySpan<T>(ptr, span.Length);
    }
  }
}