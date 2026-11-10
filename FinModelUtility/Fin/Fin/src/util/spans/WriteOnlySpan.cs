using System;
using System.Runtime.CompilerServices;

namespace fin.util.spans;

public readonly unsafe ref struct WriteOnlySpan<T> where T : unmanaged {
  private readonly T* pointer_;
  private readonly int length_;

  public T* Pointer => this.pointer_;
  public int Length => this.length_;

  public WriteOnlySpan(T* ptr, int length) {
    // Guard.IsGreaterThan(length, 0);

    this.pointer_ = ptr;
    this.length_ = length;
  }

  public T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      //Guard.IsInRange(index, 0, this.length_);
      unsafe {
        this.pointer_[index] = value;
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public WriteOnlySpan<T> Slice(int length) => this.Slice(0, length);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe WriteOnlySpan<T> Slice(int offset, int length) {
    /*if (offset + length > this.length_) {
      ThrowHelper.ThrowArgumentOutOfRangeException();
    }*/
    return new WriteOnlySpan<T>(this.pointer_ + offset, length);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe WriteOnlySpan<TTo> Cast<TTo>() where TTo : unmanaged {
    // Use unsigned integers - unsigned division by constant (especially by power of 2)
    // and checked casts are faster and smaller.
    uint fromSize = (uint) sizeof(T);
    uint toSize = (uint) sizeof(TTo);
    uint fromLength = (uint) this.length_;
    int toLength;
    if (fromSize == toSize) {
      // Special case for same size types - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
      // should be optimized to just `length` but the JIT doesn't do that today.
      toLength = (int) fromLength;
    } else if (fromSize == 1) {
      // Special case for byte sized TFrom - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
      // becomes `(ulong)fromLength / (ulong)toSize` but the JIT can't narrow it down to `int`
      // and can't eliminate the checked cast. This also avoids a 32 bit specific issue,
      // the JIT can't eliminate long multiply by 1.
      toLength = (int) (fromLength / toSize);
    } else {
      // Ensure that casts are done in such a way that the JIT is able to "see"
      // the uint->ulong casts and the multiply together so that on 32 bit targets
      // 32x32to64 multiplication is used.
      ulong toLengthUInt64
          = (ulong) fromLength * (ulong) fromSize / (ulong) toSize;
      toLength = checked((int) toLengthUInt64);
    }

    return new WriteOnlySpan<TTo>((TTo*) this.Pointer, toLength);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator WriteOnlySpan<T>(Span<T> span) {
    fixed (T* ptr = &span[0]) {
      return new WriteOnlySpan<T>(ptr, span.Length);
    }
  }
}