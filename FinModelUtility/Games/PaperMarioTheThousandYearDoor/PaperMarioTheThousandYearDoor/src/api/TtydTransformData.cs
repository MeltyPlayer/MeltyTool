using fin.model;

namespace ttyd.api;

public struct TtydTransformData<TVec, TRotation> {
  public required bool IsJoint { get; init; }

  public required TtydTransformJointData<TVec, TRotation> JointData {
    get;
    init;
  }

  public required TtydTransformNonJointData<TVec, TRotation> NonJointData {
    get;
    init;
  }
}

public struct TtydTransformJointData<TVec, TRotation> {
  public required TVec Translation { get; init; }
  public required TVec UndoParentScale { get; init; }
  public required TRotation Rotation1 { get; init; }
  public required TRotation Rotation2 { get; init; }
  public required TVec Scale { get; init; }
}

public struct TtydTransformNonJointData<TVec, TRotation> {
  public required TVec Translation { get; init; }

  public required TVec ApplyRotationCenterAndTranslation { get; init; }
  public required TRotation Rotation { get; init; }
  public required TVec UndoRotationCenter { get; init; }

  public required TVec ApplyScaleCenterAndTranslation { get; init; }
  public required TVec Scale { get; init; }
  public required TVec UndoScaleCenter { get; init; }
}