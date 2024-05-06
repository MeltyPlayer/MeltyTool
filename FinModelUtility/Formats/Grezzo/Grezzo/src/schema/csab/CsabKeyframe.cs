namespace grezzo.schema.csab {
  public readonly record struct CsabKeyframe {
    public uint Time { get; init; }
    public float Value { get; init; }

    public float? IncomingTangent { get; init; }
    public float? OutgoingTangent { get; init; }
  }
}
