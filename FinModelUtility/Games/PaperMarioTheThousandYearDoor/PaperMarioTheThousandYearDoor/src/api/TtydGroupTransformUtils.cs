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
      var rotationDegrees1
          = new Vector3(groupTransforms.Slice(6, 3));

      Func<Matrix4x4, Matrix4x4, Matrix4x4> combineMatrices
          = (lhs, rhs) => rhs * lhs;

      var deg2Rad = MathF.PI / 180;
      if (ttydGroup.IsJoint) {
        var rotationDegrees2
            = new Vector3(groupTransforms.Slice(9, 3));

        var jointMatrix = Matrix4x4.CreateTranslation(translation);

        if (ttydGroupToParent.TryGetValue(ttydGroup, out var parentTtydGroup)) {
          if (parentTtydGroup.IsJoint) {
            var parentScale
                = new Vector3(allGroupTransforms.Slice(
                                  parentTtydGroup.TransformBaseIndex + 3,
                                  3));
            jointMatrix = combineMatrices(
                jointMatrix,
                Matrix4x4.CreateScale(new Vector3(1 / parentScale.X,
                                                  1 / parentScale.Y,
                                                  1 / parentScale.Z)));
          }
        }

        jointMatrix = combineMatrices(jointMatrix,
                                      SystemMatrix4x4Util.FromRotation(
                                          QuaternionUtil.CreateZyxRadians(
                                              rotationDegrees2 *
                                              deg2Rad)));
        jointMatrix = combineMatrices(jointMatrix,
                                      SystemMatrix4x4Util.FromRotation(
                                          QuaternionUtil.CreateZyxRadians(
                                              rotationDegrees1 *
                                              deg2Rad *
                                              2)));

        jointMatrix
            = combineMatrices(jointMatrix, Matrix4x4.CreateScale(scale));

        return jointMatrix;
      }

      var rotationCenter
          = new Vector3(groupTransforms.Slice(12, 3));
      var scaleCenter
          = new Vector3(groupTransforms.Slice(15, 3));
      var rotationTranslation
          = new Vector3(groupTransforms.Slice(18, 3));
      var scaleTranslation
          = new Vector3(groupTransforms.Slice(21, 3));

      var nonJointMatrix = Matrix4x4.CreateTranslation(translation);

      nonJointMatrix = combineMatrices(
          nonJointMatrix,
          Matrix4x4.CreateTranslation(rotationCenter + rotationTranslation));
      nonJointMatrix
          = combineMatrices(nonJointMatrix,
                            SystemMatrix4x4Util.FromRotation(
                                QuaternionUtil.CreateZyxRadians(
                                    rotationDegrees1 * 2 * deg2Rad)));
      nonJointMatrix
          = combineMatrices(nonJointMatrix,
                            Matrix4x4.CreateTranslation(-rotationCenter));

      nonJointMatrix = combineMatrices(
          nonJointMatrix,
          Matrix4x4.CreateTranslation(scaleCenter + scaleTranslation));
      nonJointMatrix
          = combineMatrices(nonJointMatrix, Matrix4x4.CreateScale(scale));
      nonJointMatrix
          = combineMatrices(nonJointMatrix,
                            Matrix4x4.CreateTranslation(-scaleCenter));

      return nonJointMatrix;
    }
  }
}