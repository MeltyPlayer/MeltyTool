using System.Numerics;


namespace vrml.schema;

public record OrientationInterpolatorNode : INode {
  public (float, Quaternion)[] Keyframes { get; init; }
}

public record PositionInterpolatorNode : INode {
  public (float, Vector3)[] Keyframes { get; init; }
}

public record TimeSensorNode : INode {
  public required float CycleInterval { get; init; }
}