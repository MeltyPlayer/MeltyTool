namespace modl.xml.level {
  public abstract class BXmlLevelTypedItemList<TType> {
    public required string Name { get; init; }
    public required TType Type { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
  }
}