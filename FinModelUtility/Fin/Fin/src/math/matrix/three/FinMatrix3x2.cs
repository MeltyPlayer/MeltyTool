using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math.floats;
using fin.util.asserts;


namespace fin.math.matrix.three;

using SystemMatrix = Matrix3x2;

public sealed class FinMatrix3x2 : IFinMatrix3x2 {
  public const int ROW_COUNT = 3;
  public const int COLUMN_COUNT = 2;
  public const int CELL_COUNT = ROW_COUNT * COLUMN_COUNT;

  internal SystemMatrix impl_;

  public SystemMatrix Impl {
    get => this.impl_;
    set => this.impl_ = value;
  }

  public static IReadOnlyFinMatrix3x2 IDENTITY =
      new FinMatrix3x2().SetIdentity();

  public FinMatrix3x2() {
    this.SetZero();
  }

  public FinMatrix3x2(IReadOnlyList<float> data) {
    Asserts.Equal(CELL_COUNT, data.Count);
    for (var i = 0; i < CELL_COUNT; ++i) {
      this[i] = data[i];
    }
  }

  public FinMatrix3x2(IReadOnlyList<double> data) {
    Asserts.Equal(CELL_COUNT, data.Count);
    for (var i = 0; i < CELL_COUNT; ++i) {
      this[i] = (float) data[i];
    }
  }

  public FinMatrix3x2(IReadOnlyFinMatrix3x2 other) => this.CopyFrom(other);
  public FinMatrix3x2(in SystemMatrix other) => this.CopyFrom(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 Clone() => new FinMatrix3x2(this);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyFrom(IReadOnlyFinMatrix3x2 other) {
    Asserts.Different(this, other, "Copying into same matrix!");

    if (other is FinMatrix3x2 otherImpl) {
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
  public void CopyFrom(in SystemMatrix other) => impl_ = other;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 SetIdentity() {
    impl_ = SystemMatrix.Identity;
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 SetZero() {
    impl_ = new SystemMatrix();
    return this;
  }

  public float this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => Unsafe.Add(ref impl_.M11, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => Unsafe.Add(ref impl_.M11, index) = value;
  }

  public float this[int row, int column] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this[FinMatrix3x2.GetIndex_(row, column)];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this[FinMatrix3x2.GetIndex_(row, column)] = value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int GetIndex_(int row, int column)
    => COLUMN_COUNT * row + column;


  // Addition
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 CloneAndAdd(IReadOnlyFinMatrix3x2 other)
    => this.Clone().AddInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 AddInPlace(IReadOnlyFinMatrix3x2 other) {
    this.AddIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddIntoBuffer(IReadOnlyFinMatrix3x2 other,
                            IFinMatrix3x2 buffer)
    => this.AddIntoBuffer(other.Impl, buffer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 CloneAndAdd(in SystemMatrix other)
    => this.Clone().AddInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 AddInPlace(in SystemMatrix other) {
    this.AddIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddIntoBuffer(in SystemMatrix other, IFinMatrix3x2 buffer)
    => buffer.Impl = SystemMatrix.Add(impl_, other);


  // Matrix Multiplication
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 CloneAndMultiply(IReadOnlyFinMatrix3x2 other)
    => this.Clone().MultiplyInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 MultiplyInPlace(IReadOnlyFinMatrix3x2 other) {
    this.MultiplyIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyIntoBuffer(
      IReadOnlyFinMatrix3x2 other,
      IFinMatrix3x2 buffer) {
    if (other is FinMatrix3x2 otherImpl &&
        buffer is FinMatrix3x2 bufferImpl) {
      bufferImpl.impl_ = SystemMatrix.Multiply(otherImpl.impl_, impl_);
      return;
    }

    for (var r = 0; r < ROW_COUNT; ++r) {
      for (var c = 0; c < COLUMN_COUNT; ++c) {
        var value = 0f;

        for (var i = 0; i < 4; ++i) {
          value += this[r, i] * other[i, c];
        }

        buffer[r, c] = value;
      }
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 CloneAndMultiply(in SystemMatrix other)
    => this.Clone().MultiplyInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 MultiplyInPlace(in SystemMatrix other) {
    this.MultiplyIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyIntoBuffer(
      in SystemMatrix other,
      IFinMatrix3x2 buffer) {
    if (buffer is FinMatrix3x2 bufferImpl) {
      bufferImpl.impl_ = SystemMatrix.Multiply(other, impl_);
      return;
    }

    for (var r = 0; r < ROW_COUNT; ++r) {
      for (var c = 0; c < COLUMN_COUNT; ++c) {
        var value = 0f;

        for (var i = 0; i < 4; ++i) {
          value += this[r, i] * other[i, c];
        }

        buffer[r, c] = value;
      }
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 CloneAndMultiply(float other)
    => this.Clone().MultiplyInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 MultiplyInPlace(float other) {
    this.MultiplyIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyIntoBuffer(float other, IFinMatrix3x2 buffer) {
    if (buffer is FinMatrix3x2 bufferImpl) {
      bufferImpl.impl_ = SystemMatrix.Multiply(impl_, other);
      return;
    }

    for (var r = 0; r < ROW_COUNT; ++r) {
      for (var c = 0; c < COLUMN_COUNT; ++c) {
        buffer[r, c] = this[r, c] * other;
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 CloneAndInvert()
    => this.Clone().InvertInPlace();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IFinMatrix3x2 InvertInPlace() {
    this.InvertIntoBuffer(this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void InvertIntoBuffer(IFinMatrix3x2 buffer) {
    if (buffer is FinMatrix3x2 bufferImpl) {
      SystemMatrix.Invert(impl_, out bufferImpl.impl_);
      return;
    }

    SystemMatrix.Invert(impl_, out var invertedSystemMatrix);
    Matrix3x2ConversionUtil.CopySystemIntoFin(invertedSystemMatrix, buffer);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyTranslationInto(out Vector2 dst)
    => dst = this.impl_.GetTranslation();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyRotationInto(out float dst)
    => dst = this.impl_.GetRotation();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopyScaleInto(out Vector2 dst) => dst = this.impl_.GetScale();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void CopySkewXRadiansInto(out float dst)
    => dst = this.impl_.GetSkewXRadians();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Decompose(out Vector2 translation,
                        out float rotation,
                        out Vector2 scale,
                        out float skewXRadians)
    => this.impl_.Decompose(out translation,
                            out rotation,
                            out scale,
                            out skewXRadians);


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj)
    => ReferenceEquals(this, obj) || this.Equals(obj);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(IReadOnlyFinMatrix3x2? other) {
    if (other == null) {
      return false;
    }

    if (other is FinMatrix3x2 otherFin) {
      return this.impl_.IsRoughly(otherFin.impl_);
    }

    for (var r = 0; r < ROW_COUNT; ++r) {
      for (var c = 0; c < COLUMN_COUNT; ++c) {
        if (!this[r, c].IsRoughly(other[r, c])) {
          return false;
        }
      }
    }

    return true;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.impl_.GetRoughHashCode();
}