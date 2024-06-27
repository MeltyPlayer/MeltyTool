namespace dat.schema.animation;

public interface IDatKeyframes {
  uint DataOffset { get; }
  uint DataLength { get; }

  int StartFrame { get; }

  JointTrackType JointTrackType { get; }
  byte ValueFlag { get; }
  byte TangentFlag { get; }

  LinkedList<(int frame, float incomingValue, float outgoingValue, float? incomingTangent, float? outgoingTangent)> Keyframes { get; }
}