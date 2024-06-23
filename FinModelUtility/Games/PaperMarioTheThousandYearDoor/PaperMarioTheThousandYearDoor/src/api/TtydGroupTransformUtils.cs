using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;

using ttyd.schema.model.blocks;

namespace ttyd.api {
  public static class TtydGroupTransformUtils {
    public static Matrix4x4 GetTransformMatrix(
        Group ttydGroup,
        IReadOnlyDictionary<Group, Group> ttydGroupToParent,
        ReadOnlySpan<float> allGroupTransforms) {
      var groupTransforms
          = allGroupTransforms.Slice(ttydGroup.TransformBaseIndex, 24);

      var translation = new Vector3(groupTransforms.Slice(0, 3));
      var scale = new Vector3(groupTransforms.Slice(3, 3));
      var rotationIn2Degrees
          = new Vector3(groupTransforms.Slice(6, 3));
      var jointPostRotationInDegrees
          = new Vector3(groupTransforms.Slice(9, 3));
      var rotationCenter
          = new Vector3(groupTransforms.Slice(12, 3));
      var scaleCenter
          = new Vector3(groupTransforms.Slice(15, 3));
      var rotationPivot
          = new Vector3(groupTransforms.Slice(18, 3));
      var scalePivot
          = new Vector3(groupTransforms.Slice(21, 3));

      Func<Matrix4x4, Matrix4x4, Matrix4x4> combineMatrices
          = (lhs, rhs) => rhs * lhs;

      var translationMatrix = Matrix4x4.CreateTranslation(translation);

      var scaleMatrix = Matrix4x4.CreateTranslation(scaleCenter + scalePivot);
      scaleMatrix = combineMatrices(scaleMatrix, Matrix4x4.CreateScale(scale));
      scaleMatrix = combineMatrices(scaleMatrix,
                                    Matrix4x4.CreateTranslation(-scaleCenter));

      var deg2Rad = MathF.PI / 180;
      var rotationMatrix
          = Matrix4x4.CreateTranslation(rotationCenter + rotationPivot);
      rotationMatrix = combineMatrices(
          rotationMatrix,
          SystemMatrix4x4Util.FromRotation(
              QuaternionUtil.CreateZyxRadians(
                  jointPostRotationInDegrees * deg2Rad)));
      rotationMatrix = combineMatrices(
          rotationMatrix,
          SystemMatrix4x4Util.FromRotation(
              QuaternionUtil.CreateZyxRadians(
                  rotationIn2Degrees * 2 * deg2Rad)));
      rotationMatrix = combineMatrices(
          rotationMatrix,
          Matrix4x4.CreateTranslation(-rotationCenter));

      if (ttydGroupToParent.TryGetValue(ttydGroup, out var parentTtydGroup)) {
        var parentScale
            = new Vector3(allGroupTransforms.Slice(
                              parentTtydGroup.TransformBaseIndex + 3,
                              3));
        var invertedParentScale
            = new Vector3(1 / parentScale.X,
                          1 / parentScale.Y,
                          1 / parentScale.Z);

        rotationMatrix
            = combineMatrices(Matrix4x4.CreateScale(invertedParentScale),
                              rotationMatrix);
      }

      var matrix = translationMatrix;
      matrix = combineMatrices(matrix, rotationMatrix);
      matrix = combineMatrices(matrix, scaleMatrix);

      return matrix;
    }
  }
}