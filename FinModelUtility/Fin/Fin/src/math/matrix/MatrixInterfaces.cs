using System;

namespace fin.math.matrix {
  // The type parameters on these matrices are kind of janky, but they allow us
  // to have consistent interfaces between 3x3 and 4x4 matrices.

  public interface IFinMatrix<TMutable, TReadOnly, TImpl>
      : IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl>
      where TMutable : IFinMatrix<TMutable, TReadOnly, TImpl>, TReadOnly
      where TReadOnly : IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> {
    new TImpl Impl { get; set; }

    void CopyFrom(TReadOnly other);
    void CopyFrom(in TImpl other);

    TMutable SetIdentity();
    TMutable SetZero();

    new float this[int row, int column] { get; set; }

    TMutable AddInPlace(TReadOnly other);
    TMutable AddInPlace(in TImpl other);
    TMutable MultiplyInPlace(TReadOnly other);
    TMutable MultiplyInPlace(in TImpl other);
    TMutable MultiplyInPlace(float other);

    TMutable InvertInPlace();
  }

  public interface IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl>
      : IEquatable<TReadOnly>
      where TMutable : IFinMatrix<TMutable, TReadOnly, TImpl>, TReadOnly
      where TReadOnly : IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> {
    TImpl Impl { get; }

    TMutable Clone();

    float this[int row, int column] { get; }

    TMutable CloneAndAdd(TReadOnly other);
    void AddIntoBuffer(TReadOnly other, TMutable buffer);

    TMutable CloneAndMultiply(TReadOnly other);
    void MultiplyIntoBuffer(TReadOnly other, TMutable buffer);

    TMutable CloneAndAdd(in TImpl other);
    void AddIntoBuffer(in TImpl other, TMutable buffer);

    TMutable CloneAndMultiply(in TImpl other);
    void MultiplyIntoBuffer(in TImpl other, TMutable buffer);

    TMutable CloneAndMultiply(float other);
    void MultiplyIntoBuffer(float other, TMutable buffer);

    TMutable CloneAndInvert();
    void InvertIntoBuffer(TMutable buffer);
  }
}