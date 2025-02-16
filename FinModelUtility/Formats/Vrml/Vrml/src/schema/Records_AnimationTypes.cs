using System.Numerics;


namespace vrml.schema;

public record OrientationInterpolatorNode : BNode {
  public (float, Quaternion)[] Keyframes { get; init; }
}

public record PositionInterpolatorNode : BNode {
  public (float, Vector3)[] Keyframes { get; init; }
}

public record TimeSensorNode : BNode {
  public required float CycleInterval { get; init; }
}