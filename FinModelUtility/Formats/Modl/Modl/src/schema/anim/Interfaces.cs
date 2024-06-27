namespace modl.schema.anim;

public interface IAnim {
  List<IBwAnimBone> AnimBones { get; }
  List<AnimBoneFrames> AnimBoneFrames { get; }
}

public interface IBwAnimBone {
  string GetIdentifier();

  uint PositionKeyframeCount { get; }
  uint RotationKeyframeCount { get; }

  float XPosDelta { get; }
  float YPosDelta { get; }
  float ZPosDelta { get; }
  float XPosMin { get; }
  float YPosMin { get; }
  float ZPosMin { get; }
}