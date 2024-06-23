using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;

namespace ttyd.api {
  public static class TtydGroupTransformUtils {
    public static Matrix4x4 GetTransformMatrix(
        ReadOnlySpan<float> sceneGraphObjectTransforms,
        bool isJoint) {
      var translation = new Vector3(sceneGraphObjectTransforms.Slice(0, 3));
      var scale = new Vector3(sceneGraphObjectTransforms.Slice(3, 3));
      var rotationIn2Degrees
          = new Vector3(sceneGraphObjectTransforms.Slice(6, 3));
      var jointPostRotationInDegrees
          = new Vector3(sceneGraphObjectTransforms.Slice(9, 3));
      var transformRotationPivot
          = new Vector3(sceneGraphObjectTransforms.Slice(12, 3));
      var transformScalePivot
          = new Vector3(sceneGraphObjectTransforms.Slice(15, 3));
      var transformRotationOffset
          = new Vector3(sceneGraphObjectTransforms.Slice(18, 3));
      var transformScaleOffset
          = new Vector3(sceneGraphObjectTransforms.Slice(21, 3));

      var matrix = Matrix4x4.Identity;

      var deg2Rad = MathF.PI / 180;

      if (isJoint) {
        matrix = Matrix4x4.CreateTranslation(translation) * matrix;
        matrix = Matrix4x4.CreateScale(scale) * matrix;
        matrix = SystemMatrix4x4Util.FromRotation(
            QuaternionUtil.CreateZyxRadians(
                rotationIn2Degrees * 2 * deg2Rad)) * matrix;
        matrix = SystemMatrix4x4Util.FromRotation(
            QuaternionUtil.CreateZyxRadians(
                jointPostRotationInDegrees * deg2Rad)) * matrix;
      } else {
        matrix = Matrix4x4.CreateTranslation(translation) * matrix;
        
        matrix = Matrix4x4.CreateTranslation(transformRotationOffset) * matrix;
        matrix = Matrix4x4.CreateScale(scale) * matrix;
        matrix = Matrix4x4.CreateTranslation(-transformScaleOffset) * matrix;

        matrix = Matrix4x4.CreateTranslation(transformRotationPivot) * matrix;
        matrix = SystemMatrix4x4Util.FromRotation(
            QuaternionUtil.CreateZyxRadians(
                rotationIn2Degrees * 2 * deg2Rad)) * matrix;
        matrix = Matrix4x4.CreateTranslation(-transformScalePivot) * matrix;
      }

      return matrix;
    }
  }
}