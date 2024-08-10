using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.util.asserts;


namespace fin.math.matrix.four;

using SystemMatrix = Matrix4x4;


public sealed class FinMatrix4x4 : IFinMatrix4x4 {
  public const int ROW_COUNT = 4;
  public const int COLUMN_COUNT = 4;
  public const int CELL_COUNT = ROW_COUNT * COLUMN_COUNT;

  internal SystemMatrix impl_;

  public SystemMatrix Impl {
    get => this.impl_;
    set => this.impl_ = value;
  }

  public static IReadOnlyFinMatrix4x4 IDENTITY =
      new FinMatrix4x4().SetIdentity();

  public FinMatrix4x4() {
    this.SetZero();
  }

  public FinMatrix4x4(IReadOnlyList<float> data) {
    Asserts.Equal(CELL_COUNT, data.Count);
    for (var i = 0; i < CELL_COUNT; ++i) {
      this[i] = data[i];
    }
  }

  public FinMatrix4x4(IReadOnlyList<double> data) {
    Asserts.Equal(CELL_COUNT, data.Count);
    for (var i = 0; i < CELL_COUNT; ++i) {
      this[i] = (float) data[i];
    }
  }

  public FinMatrix4x4(ReadOnlySpan<float> data) {
    Asserts.Equal(CELL_COUNT, data.Length);
    for (var i = 0; i < CELL_COUNT; ++i) {
      this[i] = data[i];
    }
  }

  public FinMatrix4x4(ReadOnlySpan<double> data) {
    Asserts.Equal(CELL_COUNT, data.Length);
    for (var i = 0; i < CELL_COUNT; ++i) {
      this[i] = (float) data[i];
    }
  }

  public FinMatrix4x4(IReadOnlyFinMatrix4x4 other) => this.CopyFrom(other);
  public FinMatrix4x4(in SystemMatrix other) => this.CopyFrom(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 Clone() => new FinMatrix4x4(this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IReadOnlyFinMatrix4x4 other) {
    Asserts.Different(this, other, "Copying into same matrix!");

    if (other is FinMatrix4x4 otherImpl) {
      this.CopyFrom(otherImpl.Impl);
    } else {
      for (var r = 0; r < ROW_COUNT; ++r) {
        for (var c = 0; c < COLUMN_COUNT; ++c) {
          this[r, c] = other[r, c];
        }
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(in SystemMatrix other) => this.impl_ = other;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 SetIdentity() {
    this.impl_ = SystemMatrix.Identity;
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 SetZero() {
    this.impl_ = new SystemMatrix();
    return this;
  }

  public float this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => Unsafe.Add(ref this.impl_.M11, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => Unsafe.Add(ref this.impl_.M11, index) = value;
  }

  public float this[int row, int column] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this[GetIndex_(row, column)];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this[GetIndex_(row, column)] = value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int GetIndex_(int row, int column)
    => COLUMN_COUNT * row + column;


  // Addition
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 CloneAndAdd(IReadOnlyFinMatrix4x4 other)
    => this.Clone().AddInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 AddInPlace(IReadOnlyFinMatrix4x4 other) {
    this.AddIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddIntoBuffer(
      IReadOnlyFinMatrix4x4 other,
      IFinMatrix4x4 buffer)
    => this.AddIntoBuffer(other.Impl, buffer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 CloneAndAdd(in SystemMatrix other)
    => this.Clone().AddInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 AddInPlace(in SystemMatrix other) {
    this.AddIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddIntoBuffer(in SystemMatrix other, IFinMatrix4x4 buffer)
    => buffer.Impl = this.impl_ + other;


  // Matrix Multiplication
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 CloneAndMultiply(IReadOnlyFinMatrix4x4 other)
    => this.Clone().MultiplyInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 MultiplyInPlace(IReadOnlyFinMatrix4x4 other) {
    this.MultiplyIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyIntoBuffer(IReadOnlyFinMatrix4x4 other,
                                 IFinMatrix4x4 buffer)
    => this.MultiplyIntoBuffer(other.Impl, buffer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 CloneAndMultiply(in SystemMatrix other)
    => this.Clone().MultiplyInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 MultiplyInPlace(in SystemMatrix other) {
    this.MultiplyIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyIntoBuffer(in SystemMatrix other,
                                 IFinMatrix4x4 buffer) {
    buffer.Impl = SystemMatrix.Multiply(other, this.impl_);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 CloneAndMultiply(float other)
    => this.Clone().MultiplyInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 MultiplyInPlace(float other) {
    this.MultiplyIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyIntoBuffer(float other, IFinMatrix4x4 buffer)
    => buffer.Impl = SystemMatrix.Multiply(this.impl_, other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 CloneAndInvert()
    => this.Clone().InvertInPlace();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 InvertInPlace() {
    this.InvertIntoBuffer(this);
    return this;
  }

  public const bool STRICT_INVERTING = true;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void InvertIntoBuffer(IFinMatrix4x4 buffer)
    => buffer.Impl = SystemMatrix4x4Util.AssertInvert(this.impl_);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 CloneAndTranspose()
    => this.Clone().TransposeInPlace();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix4x4 TransposeInPlace() {
    this.TransposeIntoBuffer(this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void TransposeIntoBuffer(IFinMatrix4x4 buffer)
    => buffer.Impl = SystemMatrix.Transpose(this.impl_);

  // Shamelessly copied from https://math.stackexchange.com/a/1463487
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTranslationInto(out Vector3 dst)
    => dst = this.impl_.Translation;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyRotationInto(out Quaternion dst) {
    this.Decompose(out _, out dst, out _);
    dst = -dst;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyScaleInto(out Vector3 dst)
    => this.Decompose(out _, out _, out dst);


  public const bool STRICT_DECOMPOSITION = true;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Decompose(out Vector3 translation,
                        out Quaternion rotation,
                        out Vector3 scale) {
    translation = default;
    scale = default;
    Asserts.True(
        SystemMatrix.Decompose(this.impl_,
            out scale,
            out rotation,
            out translation) ||
        !STRICT_DECOMPOSITION,
        "Failed to decompose matrix!");
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj)
    => ReferenceEquals(this, obj) || this.Equals(obj as IReadOnlyFinMatrix4x4);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(IReadOnlyFinMatrix4x4? other) {
    if (other == null) {
      return false;
    }

    return this.Impl.IsRoughly(other.Impl);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.impl_.GetRoughHashCode();

  public override string ToString() => this.impl_.ToString();
}