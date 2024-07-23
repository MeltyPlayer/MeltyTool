using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.math.matrix.four;

public interface IMatrix4x4Stack {
  Matrix4x4 Top { get; set; }

  Matrix4x4 Pop();
  void Push(Matrix4x4 value);
  void Push();

  void SetIdentity();
  void MultiplyInPlace(Matrix4x4 other);
}

public class Matrix4x4Stack : IMatrix4x4Stack {
  private readonly Stack<Matrix4x4> impl_ = new([Matrix4x4.Identity]);

  public Matrix4x4 Top {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.impl_.Peek();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set {
      this.impl_.Pop();
      this.impl_.Push(value);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Matrix4x4 Pop() => this.impl_.Pop();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Push(Matrix4x4 value) => this.impl_.Push(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Push() => this.impl_.Push(this.impl_.Peek());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetIdentity() => this.Top = Matrix4x4.Identity;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyInPlace(Matrix4x4 other) => this.Top = other * this.Top;
}