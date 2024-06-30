namespace dat.schema.animation;

public interface IDatKeyframes {
  uint DataOffset { get; }
  uint DataLength { get; }

  int StartFrame { get; }

  JointTrackType JointTrackType { get; }
  byte ValueFlag { get; }
  byte TangentFlag { get; }

  LinkedList<DatKeyframe> Keyframes { get; }
}