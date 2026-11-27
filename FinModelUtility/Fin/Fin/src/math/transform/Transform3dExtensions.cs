using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;
using fin.math.xyz;


namespace fin.math.transform;

public static class Transform3dExtensions {
  extension(ITransform3d transform) {
    public ITransform3d SetMatrix(IReadOnlyFinMatrix4x4 matrix)
      => transform.SetMatrix(matrix.Impl);

    public ITransform3d SetMatrix(Matrix4x4 matrix) {
      matrix.AssertDecompose(out var translation,
                             out var quaternion,
                             out var scale);

      transform.LocalTranslation = translation;
      transform.LocalRotation = quaternion;
      transform.LocalScale = scale;

      return transform;
    }

    public ITransform3d SetTranslation(IReadOnlyXyz xyz)
      => transform.SetTranslation(xyz.X, xyz.Y, xyz.Z);

    public ITransform3d SetTranslation(float x,
                                       float y,
                                       float z) {
      transform.LocalTranslation = new Vector3(x, y, z);
      return transform;
    }

    public ITransform3d SetRotationRadians(Vector3 xyz)
      => transform.SetRotationRadians(xyz.X, xyz.Y, xyz.Z);

    public ITransform3d SetRotationRadians(IReadOnlyXyz xyz)
      => transform.SetRotationRadians(xyz.X, xyz.Y, xyz.Z);

    public ITransform3d SetRotationRadians(float x,
                                           float y,
                                           float z) {
      return transform.SetRotation(
          QuaternionUtil.CreateZyxRadians(x, y, z));
    }

    public ITransform3d SetRotationDegrees(Vector3 xyz)
      => transform.SetRotationDegrees(xyz.X, xyz.Y, xyz.Z);

    public ITransform3d SetRotationDegrees(IReadOnlyXyz xyz)
      => transform.SetRotationDegrees(xyz.X, xyz.Y, xyz.Z);

    public ITransform3d SetRotationDegrees(float x, float y, float z)
      => transform.SetRotationRadians(x * FinTrig.DEG_2_RAD,
                                      y * FinTrig.DEG_2_RAD,
                                      z * FinTrig.DEG_2_RAD);

    public ITransform3d SetRotation(Quaternion quaternion) {
      transform.LocalRotation = quaternion;
      return transform;
    }

    public ITransform3d SetScale(Vector3 xyz) {
      transform.LocalScale = xyz;
      return transform;
    }

    public ITransform3d SetScale(IReadOnlyXyz xyz)
      => transform.SetScale(xyz.X, xyz.Y, xyz.Z);

    public ITransform3d SetScale(float x,
                                 float y,
                                 float z)
      => transform.SetScale(new Vector3(x, y, z));
  }
}