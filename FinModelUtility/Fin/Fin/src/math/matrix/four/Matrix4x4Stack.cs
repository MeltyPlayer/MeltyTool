using System.Numerics;
using System.Runtime.CompilerServices;

using fin.data.stacks;

namespace fin.math.matrix.four;

public interface IMatrix4x4Stack {
  ref Matrix4x4 Top { get; }

  void Pop();
  void Push(in Matrix4x4 value);
  void Push();

  void SetIdentity();
  void MultiplyInPlace(in Matrix4x4 other);
}

public sealed class Matrix4x4Stack : IMatrix4x4Stack {
  private readonly RefStack<Matrix4x4> impl_;

  public Matrix4x4Stack() {
    this.impl_ = new RefStack<Matrix4x4>();
    this.impl_.Push(Matrix4x4.Identity);
  }

  public ref Matrix4x4 Top {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => ref this.impl_.Top;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Pop() => this.impl_.Pop();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Push(in Matrix4x4 value) => this.impl_.Push(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Push() => this.impl_.Push(in this.impl_.Top);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetIdentity() => this.Top = Matrix4x4.Identity;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void MultiplyInPlace(in Matrix4x4 other) {
    ref var top = ref this.Top;
    top = other * top;
  }
}