namespace vrml.schema;

public enum Justify {
  BEGIN,
  MIDDLE,
  END
}

public record FontStyleNode : BNode {
  public string? Family { get; init; }
  public Justify Justify { get; init; }
  public float? Size { get; init; }
  public string Style { get; init; }
}