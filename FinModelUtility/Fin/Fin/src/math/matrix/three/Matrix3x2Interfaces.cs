using System.Numerics;

using schema.readOnly;

namespace fin.math.matrix.three;

[GenerateReadOnly]
public partial interface IFinMatrix3x2
    : IFinMatrix<IFinMatrix3x2, IReadOnlyFinMatrix3x2, Matrix3x2> {
  [Const]
  void CopyTranslationInto(out Vector2 dst);

  [Const]
  void CopyRotationInto(out float dst);

  [Const]
  void CopyScaleInto(out Vector2 dst);

  [Const]
  void CopySkewXRadiansInto(out float dst);

  [Const]
  void Decompose(out Vector2 translation,
                 out float rotation,
                 out Vector2 scale,
                 out float skewXRadians);
}