namespace ttyd.api;

public class TtydTransformData<T> {
  public required bool IsJoint { get; init; }
  public required TtydTransformJointData<T>? JointData { get; init; }
  public required TtydTransformNonJointData<T>? NonJointData { get; init; }
}

public class TtydTransformJointData<T> {
  public required T Translation { get; init; }
  public required T ParentScale { get; init; }
  public required T Rotation1 { get; init; }
  public required T Rotation2 { get; init; }
  public required T Scale { get; init; }
}

public class TtydTransformNonJointData<T> {
  public required T Translation { get; init; }

  public required T ApplyRotationCenterAndTranslation { get; init; }
  public required T Rotation { get; init; }
  public required T UndoRotationCenter { get; init; }

  public required T ApplyScaleCenterAndTranslation { get; init; }
  public required T Scale { get; init; }
  public required T UndoScaleCenter { get; init; }
}