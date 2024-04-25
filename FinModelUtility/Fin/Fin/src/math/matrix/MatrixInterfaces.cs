using schema.readOnly;

namespace fin.math.matrix {
  // The type parameters on these matrices are kind of janky, but they allow us
  // to have consistent interfaces between 3x3 and 4x4 matrices.

  [GenerateReadOnly]
  public partial interface IFinMatrix<[KeepMutableType] TMutable, TReadOnly,
                                      TImpl>
      where TMutable : IFinMatrix<TMutable, TReadOnly, TImpl>, TReadOnly
      where TReadOnly : IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> {
    TImpl Impl { get; set; }

    void CopyFrom(TReadOnly other);
    void CopyFrom(in TImpl other);

    TMutable SetIdentity();
    TMutable SetZero();

    float this[int row, int column] { get; set; }

    TMutable AddInPlace(TReadOnly other);
    TMutable AddInPlace(in TImpl other);
    TMutable MultiplyInPlace(TReadOnly other);
    TMutable MultiplyInPlace(in TImpl other);
    TMutable MultiplyInPlace(float other);

    TMutable InvertInPlace();

    [Const]
    TMutable Clone();

    [Const]
    TMutable CloneAndAdd(TReadOnly other);

    [Const]
    void AddIntoBuffer(TReadOnly other, TMutable buffer);

    [Const]
    TMutable CloneAndMultiply(TReadOnly other);

    [Const]
    void MultiplyIntoBuffer(TReadOnly other, TMutable buffer);

    [Const]
    TMutable CloneAndAdd(in TImpl other);

    [Const]
    void AddIntoBuffer(in TImpl other, TMutable buffer);

    [Const]
    TMutable CloneAndMultiply(in TImpl other);

    [Const]
    void MultiplyIntoBuffer(in TImpl other, TMutable buffer);

    [Const]
    TMutable CloneAndMultiply(float other);

    [Const]
    void MultiplyIntoBuffer(float other, TMutable buffer);

    [Const]
    TMutable CloneAndInvert();

    [Const]
    void InvertIntoBuffer(TMutable buffer);
  }
}