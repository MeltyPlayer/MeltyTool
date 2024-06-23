using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;

namespace ttyd.api {
  public static class TtydGroupTransformUtils {
    public static Matrix4x4 GetTransformMatrix(
        ReadOnlySpan<float> sceneGraphObjectTransforms) {
      // Translate by v1
      var matrix = SystemMatrix4x4Util.FromTranslation(
          sceneGraphObjectTransforms[0],
          sceneGraphObjectTransforms[1],
          sceneGraphObjectTransforms[2]);

      // Translate by v7
      matrix = SystemMatrix4x4Util.FromTranslation(
                   sceneGraphObjectTransforms[18],
                   sceneGraphObjectTransforms[19],
                   sceneGraphObjectTransforms[20]) *
               matrix;

      // Scale by v2
      matrix = SystemMatrix4x4Util.FromScale(
                   sceneGraphObjectTransforms[3],
                   sceneGraphObjectTransforms[4],
                   sceneGraphObjectTransforms[5]) *
               matrix;

      // Translate by -v8
      matrix = SystemMatrix4x4Util.FromTranslation(
                   -sceneGraphObjectTransforms[21],
                   -sceneGraphObjectTransforms[22],
                   -sceneGraphObjectTransforms[23]) *
               matrix;

      // Rotate by v4
      matrix = SystemMatrix4x4Util.FromRotation(
                   QuaternionUtil.CreateZyx(
                       float.DegreesToRadians(sceneGraphObjectTransforms[9]),
                       float.DegreesToRadians(sceneGraphObjectTransforms[10]),
                       float.DegreesToRadians(
                           sceneGraphObjectTransforms[11]))) *
               matrix;

      // Translate by v5
      matrix = SystemMatrix4x4Util.FromTranslation(
                   sceneGraphObjectTransforms[12],
                   sceneGraphObjectTransforms[13],
                   sceneGraphObjectTransforms[14]) *
               matrix;

      // Rotate by 2 * v3
      matrix = SystemMatrix4x4Util.FromRotation(
                   QuaternionUtil.CreateZyx(
                       float.DegreesToRadians(
                           2 * sceneGraphObjectTransforms[6]),
                       float.DegreesToRadians(
                           2 * sceneGraphObjectTransforms[7]),
                       float.DegreesToRadians(
                           2 * sceneGraphObjectTransforms[8]))) *
               matrix;

      // Translate by -v6
      matrix = SystemMatrix4x4Util
                   .FromTranslation(
                       -sceneGraphObjectTransforms[15],
                       -sceneGraphObjectTransforms[16],
                       -sceneGraphObjectTransforms[17]) *
               matrix;

      return matrix;
    }
  }
}