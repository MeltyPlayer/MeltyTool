namespace modl.xml.level {
  public class XmlLevelObject {
    public required string Type { get; init; }
    public required string Id { get; init; }
    public required IReadOnlyList<IXmlLevelObjectField> Fields { get; init; }
  }
}