namespace vrml.schema;

public enum Family {
  SANS,
  SERIF,
  TYPEWRITER,
}

public enum Justify {
  BEGIN,
  MIDDLE,
  END
}

public enum Style {
  BOLD,
  BOLD_ITALIC,
  ITALIC,
  PLAIN,
}

public record FontStyleNode : BNode {
  public Family Family { get; set; } = Family.SERIF;
  public Justify Justify { get; set; } = Justify.BEGIN;
  public float Size { get; set; } = 1;
  public Style Style { get; set; } = Style.PLAIN;
}