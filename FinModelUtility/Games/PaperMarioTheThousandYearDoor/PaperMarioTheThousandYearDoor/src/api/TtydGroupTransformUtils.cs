using System.Numerics;

using ttyd.schema.model.blocks;

namespace ttyd.api;

public static class TtydGroupTransformUtils {
  public static TtydTransformData<Vector3, Vector3> GetTransformData(
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

    return GetTransformData(group, groupTransforms, parentGroupScale);
  }

  public static TtydTransformData<Vector3, Vector3> GetTransformData(
      Group group,
      IReadOnlyDictionary<Group, Group> groupToParent,
      IGroupTransformBakedFrames keyframes,
      int frame) {
    Span<float> groupBuffer = stackalloc float[24];
    keyframes.GetTransformsAtFrame(group, frame, groupBuffer);

    Vector3? parentGroupScale = null;
    if (group.IsJoint &&
        groupToParent.TryGetValue(group, out var parentGroup)) {
      if (parentGroup.IsJoint) {
        Span<float> parentBuffer = stackalloc float[3];
        keyframes.GetTransformsAtFrame(parentGroup, frame, 3, parentBuffer);
        parentGroupScale = new Vector3(parentBuffer);
      }
    }

    return GetTransformData(group, groupBuffer, parentGroupScale);
  }

  public static TtydTransformData<Vector3, Vector3> GetTransformData(
      Group group,
      ReadOnlySpan<float> groupTransforms,
      Vector3? parentGroupScale) {
    var translation = new Vector3(groupTransforms[..3]);
    var scale = new Vector3(groupTransforms.Slice(3, 3));

    var deg2Rad = MathF.PI / 180;
    var rotationRadians1
        = new Vector3(groupTransforms.Slice(6, 3)) * 2 * deg2Rad;

    if (group.IsJoint) {
      var rotationRadians2
          = new Vector3(groupTransforms.Slice(9, 3)) * deg2Rad;

      return new TtydTransformData<Vector3, Vector3> {
          IsJoint = true,
          JointData = new TtydTransformJointData<Vector3, Vector3> {
              Translation = translation,
              UndoParentScale = parentGroupScale != null
                  ? new Vector3(1 / parentGroupScale.Value.X,
                                1 / parentGroupScale.Value.Y,
                                1 / parentGroupScale.Value.Z)
                  : Vector3.One,
              Rotation1 = rotationRadians1,
              Rotation2 = rotationRadians2,
              Scale = scale,
          },
          NonJointData = default,
      };
    }

    var rotationCenter
        = new Vector3(groupTransforms.Slice(12, 3));
    var scaleCenter
        = new Vector3(groupTransforms.Slice(15, 3));
    var rotationTranslation
        = new Vector3(groupTransforms.Slice(18, 3));
    var scaleTranslation
        = new Vector3(groupTransforms.Slice(21, 3));

    return new TtydTransformData<Vector3, Vector3> {
        IsJoint = false,
        JointData = default,
        NonJointData = new TtydTransformNonJointData<Vector3, Vector3> {
            Translation = translation,
            ApplyRotationCenterAndTranslation
                = rotationCenter + rotationTranslation,
            Rotation = rotationRadians1,
            UndoRotationCenter = -rotationCenter,
            ApplyScaleCenterAndTranslation = scaleCenter + scaleTranslation,
            Scale = scale,
            UndoScaleCenter = -scaleCenter,
        },
    };
  }
}