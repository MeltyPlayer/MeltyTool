using fin.model;
using fin.util.enumerables;

namespace fin.ui.rendering.gl.model;

public readonly struct MergedPrimitive {
  public required PrimitiveType PrimitiveType { get; init; }
  public required bool IsFlipped { get; init; }

  public required IEnumerable<IEnumerable<IReadOnlyVertex>> Vertices {
    get;
    init;
  }
}

public class PrimitiveMerger {
  public bool TryToMergePrimitives(
      IList<IReadOnlyPrimitive> primitives,
      out MergedPrimitive mergedPrimitive) {
      mergedPrimitive = default;
      if (primitives.Count == 0) {
        return false;
      }

      if (primitives.Count == 1) {
        var primitive = primitives[0];
        mergedPrimitive = new MergedPrimitive {
            PrimitiveType = primitive.Type,
            IsFlipped = primitive.VertexOrder == VertexOrder.FLIP,
            Vertices = primitive.Vertices.Yield(),
        };
        return true;
      }

      var primitiveTypes = primitives.Select(primitive => primitive.Type)
                                     .Distinct()
                                     .ToArray();
      if (primitiveTypes is [PrimitiveType.LINES or PrimitiveType.POINTS]) {
        var primitiveType = primitiveTypes.First();
        mergedPrimitive = new MergedPrimitive {
            PrimitiveType = primitiveType,
            Vertices = primitives.SelectMany(primitive => primitive.Vertices)
                                 .Yield(),
            IsFlipped = false,
        };

        return true;
      }

      var flippedType =
          primitives.Select(primitive
                                => primitive.VertexOrder == VertexOrder.FLIP)
                    .Distinct()
                    .ToArray();
      if (primitiveTypes is
              [PrimitiveType.TRIANGLE_STRIP, PrimitiveType.TRIANGLE_FAN] &&
          flippedType.Length == 1) {
        mergedPrimitive = new MergedPrimitive {
            PrimitiveType = primitiveTypes[0],
            Vertices = primitives.Select(primitive => primitive.Vertices),
            IsFlipped = flippedType[0],
        };

        return true;
      }

      mergedPrimitive = new MergedPrimitive {
          PrimitiveType = PrimitiveType.TRIANGLES,
          Vertices = primitives
                     .SelectMany(primitive => primitive.GetOrderedTriangleVertices())
                     .Yield(),
          IsFlipped = false,
      };

      return true;
    }
}