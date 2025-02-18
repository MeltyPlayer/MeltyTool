namespace vrml.schema;

public enum Family {
  SANS,
  SERIF,
  TYPEWRITER,
}

public enum Justify {
  BEGIN,
  FIRST,
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
  public Justify MajorJustify { get; set; } = Justify.BEGIN;
  public Justify MinorJustify { get; set; } = Justify.FIRST;
  public float Size { get; set; } = 1;
  public Style Style { get; set; } = Style.PLAIN;
}