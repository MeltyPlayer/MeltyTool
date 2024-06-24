using System.Numerics;

using ttyd.schema.model.blocks;

namespace ttyd.api {
  public static class TtydGroupTransformUtils {
    public static Matrix4x4 GetTransformMatrix(
        Group group,
        IReadOnlyDictionary<Group, Group> groupToParent,
        ReadOnlySpan<float> allGroupTransforms) {
      var groupTransforms
          = allGroupTransforms.Slice(group.TransformBaseIndex, 24);

      Vector3? parentGroupScale = null;
      if (group.IsJoint &&
          groupToParent.TryGetValue(group, out var parentGroup)) {
        if (parentGroup.IsJoint) {
          parentGroupScale = new Vector3(
                  allGroupTransforms.Slice(parentGroup.TransformBaseIndex + 3,
                                           3));
        }
      }

      return GetTransformMatrix(group, groupTransforms, parentGroupScale);
    }

    public static Matrix4x4 GetTransformMatrix(
        Group group,
        IReadOnlyDictionary<Group, Group> groupToParent,
        IGroupTransformKeyframes keyframes,
        int frame) {
      Span<float> groupBuffer = stackalloc float[24];
      keyframes.GetTransformsAtFrame(group, frame, groupBuffer);

      Vector3? parentGroupScale = null;
      if (group.IsJoint &&
          groupToParent.TryGetValue(group, out var parentGroup)) {
        if (parentGroup.IsJoint) {
          Span<float> parentBuffer = stackalloc float[3];
          keyframes.GetTransformsAtFrame(parentGroup, frame, 3, 3, parentBuffer);
          parentGroupScale = new Vector3(parentBuffer);
        }
      }

      return GetTransformMatrix(group, groupBuffer, parentGroupScale);
    }

    public static Matrix4x4 GetTransformMatrix(
        Group group,
        ReadOnlySpan<float> groupTransforms,
        Vector3? parentGroupScale) {
      var translation = new Vector3(groupTransforms.Slice(0, 3));
      var scale = new Vector3(groupTransforms.Slice(3, 3));

      var deg2Rad = MathF.PI / 180;
      var rotationDegrees1
          = new Vector3(groupTransforms.Slice(6, 3)) * 2 * deg2Rad;

      Func<Matrix4x4, Matrix4x4, Matrix4x4> combineMatrices
          = (lhs, rhs) => rhs * lhs;
      Func<Matrix4x4, Vector3, Matrix4x4> applyRotationMatrixes
          = (mtx, rotation) => {
              mtx = combineMatrices(mtx, Matrix4x4.CreateRotationZ(rotation.Z));
              mtx = combineMatrices(mtx, Matrix4x4.CreateRotationY(rotation.Y));
              mtx = combineMatrices(mtx, Matrix4x4.CreateRotationX(rotation.X));

              return mtx;
            };

      if (group.IsJoint) {
        var rotationDegrees2
            = new Vector3(groupTransforms.Slice(9, 3)) * deg2Rad;

        var jointMatrix = Matrix4x4.CreateTranslation(translation);

        if (parentGroupScale != null) {
          jointMatrix = combineMatrices(
              jointMatrix,
              Matrix4x4.CreateScale(new Vector3(1 / parentGroupScale.Value.X,
                                                1 / parentGroupScale.Value.Y,
                                                1 / parentGroupScale.Value.Z)));
        }

        jointMatrix = applyRotationMatrixes(jointMatrix, rotationDegrees2);
        jointMatrix = applyRotationMatrixes(jointMatrix, rotationDegrees1);

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
      nonJointMatrix = applyRotationMatrixes(nonJointMatrix, rotationDegrees1);
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