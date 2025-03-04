namespace gx.displayList;

public record GxPrimitive(
    GxPrimitiveType PrimitiveType,
    IList<GxVertex> Vertices);