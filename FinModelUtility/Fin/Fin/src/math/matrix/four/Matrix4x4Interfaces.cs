using System.Numerics;

using schema.readOnly;

namespace fin.math.matrix.four;

[GenerateReadOnly]
public partial interface IFinMatrix4x4
    : IFinMatrix<IFinMatrix4x4, IReadOnlyFinMatrix4x4, Matrix4x4> {
IFinMatrix4x4 TransposeInPlace();

[Const]
IFinMatrix4x4 CloneAndTranspose();

[Const]
void TransposeIntoBuffer(IFinMatrix4x4 buffer);

[Const]
void CopyTranslationInto(out Vector3 dst);

[Const]
void CopyRotationInto(out Quaternion dst);

[Const]
void CopyScaleInto(out Vector3 dst);

[Const]
void Decompose(out Vector3 translation,
               out Quaternion rotation,
               out Vector3 scale);
}