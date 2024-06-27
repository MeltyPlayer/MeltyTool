using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.math.matrix.four;

public class FinVector4 {
  private Vector4 impl_;

  public FinVector4() { }

  public FinVector4(float x, float y, float z, float w)
    => this.impl_ = new Vector4(x, y, z, w);

  public FinVector4(FinVector4 other) => other.CopyInto(this);

  public FinVector4 Clone() => new FinVector4(this);

  public void CopyInto(FinVector4 other)
    => other.impl_ = this.impl_;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Set(float x, float y, float z, float w)
    => this.impl_ = new Vector4(x, y, z, w);


  // Accessing values
  public float this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => Unsafe.Add(ref this.impl_.X, index);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => Unsafe.Add(ref this.impl_.X, index) = value;
  }

  public float X {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.X;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.impl_.X = value;
  }

  public float Y {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Y;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.impl_.Y = value;
  }

  public float Z {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Z;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.impl_.Z = value;
  }

  public float W {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.W;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this.impl_.W = value;
  }


  // Normalizing
  public float Length => this.impl_.Length();

  public FinVector4 CloneAndNormalize() => this.Clone().NormalizeInPlace();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FinVector4 NormalizeInPlace() {
    this.impl_ = Vector4.Normalize(this.impl_);
    return this;
  }


  // Addition
  public FinVector4 CloneAndAdd(FinVector4 other)
    => this.Clone().AddInPlace(other);

  public FinVector4 AddInPlace(FinVector4 other) {
    this.AddIntoBuffer(other, this);
    return this;
  }

  public void AddIntoBuffer(FinVector4 other, FinVector4 buffer)
    => buffer.impl_ = Vector4.Add(this.impl_, other.impl_);


  // Multiplication
  public FinVector4 CloneAndMultiply(float other)
    => this.Clone().MultiplyInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FinVector4 MultiplyInPlace(float other) {
    this.MultiplyIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyIntoBuffer(float other, FinVector4 buffer)
    => buffer.impl_ = Vector4.Multiply(other, this.impl_);


  // Vector Multiplication
  public float Dot(FinVector4 other) => Vector4.Dot(this.impl_, other.impl_);


  // Matrix Multiplication
  public FinVector4 CloneAndMultiply(IReadOnlyFinMatrix4x4 other)
    => this.Clone().MultiplyInPlace(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FinVector4 MultiplyInPlace(IReadOnlyFinMatrix4x4 other) {
    this.MultiplyIntoBuffer(other, this);
    return this;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyIntoBuffer(
      IReadOnlyFinMatrix4x4 other,
      FinVector4 buffer)
    => buffer.impl_ = Vector4.Transform(this.impl_, other.Impl);
}