using System.Numerics;

namespace fin.math.matrix.three;

public interface IFinMatrix3x2
    : IFinMatrix<IFinMatrix3x2, IReadOnlyFinMatrix3x2, Matrix3x2>,
      IReadOnlyFinMatrix3x2;

public interface IReadOnlyFinMatrix3x2
    : IReadOnlyFinMatrix<IFinMatrix3x2, IReadOnlyFinMatrix3x2, Matrix3x2> {
  void CopyTranslationInto(out Vector2 dst);
  void CopyRotationInto(out float dst);
  void CopyScaleInto(out Vector2 dst);
  void CopySkewXRadiansInto(out float dst);

  void Decompose(out Vector2 translation,
                 out float rotation,
                 out Vector2 scale,
                 out float skewXRadians);
}