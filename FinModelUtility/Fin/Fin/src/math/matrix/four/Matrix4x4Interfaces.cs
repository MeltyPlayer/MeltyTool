using System.Numerics;

using fin.model;

namespace fin.math.matrix.four {
  public interface IFinMatrix4x4
      : IFinMatrix<IFinMatrix4x4, IReadOnlyFinMatrix4x4, Matrix4x4>,
        IReadOnlyFinMatrix4x4 {
    IFinMatrix4x4 TransposeInPlace();
  }

  public interface IReadOnlyFinMatrix4x4
      : IReadOnlyFinMatrix<IFinMatrix4x4, IReadOnlyFinMatrix4x4, Matrix4x4> {
    IFinMatrix4x4 CloneAndTranspose();
    void TransposeIntoBuffer(IFinMatrix4x4 buffer);

    void CopyTranslationInto(out Position dst);
    void CopyRotationInto(out Quaternion dst);
    void CopyScaleInto(out Scale dst);

    void Decompose(out Position translation,
                   out Quaternion rotation,
                   out Scale scale);
  }
}