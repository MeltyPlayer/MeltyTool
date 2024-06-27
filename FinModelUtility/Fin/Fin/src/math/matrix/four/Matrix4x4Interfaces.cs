using System.Numerics;

namespace fin.math.matrix.four;

public interface IFinMatrix4x4
    : IFinMatrix<IFinMatrix4x4, IReadOnlyFinMatrix4x4, Matrix4x4>,
      IReadOnlyFinMatrix4x4 {
  IFinMatrix4x4 TransposeInPlace();
}

public interface IReadOnlyFinMatrix4x4
    : IReadOnlyFinMatrix<IFinMatrix4x4, IReadOnlyFinMatrix4x4, Matrix4x4> {
  IFinMatrix4x4 CloneAndTranspose();
  void TransposeIntoBuffer(IFinMatrix4x4 buffer);

  void CopyTranslationInto(out Vector3 dst);
  void CopyRotationInto(out Quaternion dst);
  void CopyScaleInto(out Vector3 dst);

  void Decompose(out Vector3 translation,
                 out Quaternion rotation,
                 out Vector3 scale);
}