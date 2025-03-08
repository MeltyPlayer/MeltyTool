namespace ttyd.api;

public struct TtydTransformData<TVec, TRotation> {
  public required TVec Translation { get; init; }

  public required TVec ApplyRotationCenterAndTranslation { get; init; }
  public required TRotation Rotation1 { get; init; }
  public required TRotation Rotation2 { get; init; }
  public required TVec UndoRotationCenter { get; init; }

  public required TVec ApplyScaleCenterAndTranslation { get; init; }
  public required TVec Scale { get; init; }
  public required TVec UndoScaleCenter { get; init; }
}