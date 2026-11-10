using fin.model;
using fin.model.util;
using fin.util.enumerables;

namespace fin.ui.rendering.gl.model;

public class MergedPrimitive {
  public required PrimitiveType PrimitiveType { get; init; }
  public required bool IsFlipped { get; init; }
  public MergedPrimitive? Base { get; set; }

  public required
      IEnumerable<(IReadOnlyPrimitive, IEnumerable<IReadOnlyVertex>)> Vertices {
    get;
    init;
  }

  public required bool RequiresSeparators { get; init; }
}

public sealed class PrimitiveMerger {
  public bool TryToMergePrimitives(
      IList<IReadOnlyPrimitive> primitives,
      out MergedPrimitive mergedPrimitive) {
    mergedPrimitive = default;
    if (primitives.Count == 0) {
      return false;
    }

    if (primitives is [IReadOnlyPrimitive primitive] &&
        primitive.Type.IsSupportedByOpenGl()) {
      mergedPrimitive = new MergedPrimitive {
          PrimitiveType = primitive.Type,
          IsFlipped = primitive.VertexOrder == VertexOrder.CLOCKWISE,
          Vertices = (primitive, (IEnumerable<IReadOnlyVertex>) primitive.Vertices)
              .Yield(),
          RequiresSeparators = false,
      };
      return true;
    }

    var primitiveTypes = primitives.Select(primitive => primitive.Type)
                                   .Distinct()
                                   .ToArray();
    if (primitiveTypes is [
            PrimitiveType.LINES
            or PrimitiveType.LINE_STRIP
            or PrimitiveType.POINTS
        ]) {
      var primitiveType = primitiveTypes.First();
      mergedPrimitive = new MergedPrimitive {
          PrimitiveType = primitiveType,
          Vertices = primitives.Select(primitive => (
                                           primitive,
                                           (IEnumerable<IReadOnlyVertex>)
                                           primitive.Vertices)),
          RequiresSeparators = false,
          IsFlipped = false,
      };

      return true;
    }

    var flippedType =
        primitives.Select(primitive
                              => primitive.VertexOrder == VertexOrder.CLOCKWISE)
                  .Distinct()
                  .ToArray();
    if (primitiveTypes is
            [PrimitiveType.TRIANGLE_STRIP, PrimitiveType.TRIANGLE_FAN] &&
        flippedType.Length == 1) {
      mergedPrimitive = new MergedPrimitive {
          PrimitiveType = primitiveTypes[0],
          Vertices = primitives.Select(primitive => (
                                           primitive,
                                           (IEnumerable<IReadOnlyVertex>)
                                           primitive.Vertices)),
          RequiresSeparators = true,
          IsFlipped = flippedType[0],
      };

      return true;
    }

    mergedPrimitive = new MergedPrimitive {
        PrimitiveType = PrimitiveType.TRIANGLES,
        Vertices = primitives
            .Select(primitive
                        => (primitive, primitive.GetOrderedTriangleVertices())),
        RequiresSeparators = false,
        IsFlipped = false,
    };

    return true;
  }
}