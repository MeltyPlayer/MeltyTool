using System.Collections.Generic;
using System;
using System.Linq;
using System.Numerics;

using fin.math.xyz;
using fin.util.enumerables;

namespace fin.model.util;

public static class SkinExtensions {
  public static TVertex AddVertex<TVertex>(
      this ISkin<TVertex> skin,
      float x,
      float y,
      float z)
      where TVertex : IReadOnlyVertex
    => skin.AddVertex(new Vector3(x, y, z));

  public static TVertex AddVertex<TVertex>(
      this ISkin<TVertex> skin,
      IReadOnlyXyz position)
      where TVertex : IReadOnlyVertex
    => skin.AddVertex(position.X, position.Y, position.Z);


  public static IEnumerable<int> GetOrderedTriangleVertexIndices(
      this IReadOnlyPrimitive primitive) {
    var pointsCount = primitive.Vertices.Count;
    switch (primitive.Type) {
      case PrimitiveType.TRIANGLES: {
        for (var v = 0; v < pointsCount; v += 3) {
          if (primitive.VertexOrder == VertexOrder.FLIP) {
            yield return v + 0;
            yield return v + 2;
            yield return v + 1;
          } else {
            yield return v + 0;
            yield return v + 1;
            yield return v + 2;
          }
        }

        break;
      }
      case PrimitiveType.TRIANGLE_STRIP: {
        for (var v = 0; v < pointsCount - 2; ++v) {
          int v1, v2, v3;
          if (v % 2 == 0) {
            v1 = v + 0;
            v2 = v + 1;
            v3 = v + 2;
          } else {
            // Switches drawing order to maintain proper winding:
            // https://www.khronos.org/opengl/wiki/Primitive
            v1 = v + 1;
            v2 = v + 0;
            v3 = v + 2;
          }

          if (primitive.VertexOrder == VertexOrder.FLIP) {
            yield return v1;
            yield return v3;
            yield return v2;
          } else {
            yield return v1;
            yield return v2;
            yield return v3;
          }
        }

        break;
      }
      case PrimitiveType.TRIANGLE_FAN: {
        // https://stackoverflow.com/a/8044252
        var firstVertex = 0;
        for (var v = 2; v < pointsCount; ++v) {
          var v1 = firstVertex;
          var v2 = v - 1;
          var v3 = v;

          if (primitive.VertexOrder == VertexOrder.FLIP) {
            yield return v1;
            yield return v3;
            yield return v2;
          } else {
            yield return v1;
            yield return v2;
            yield return v3;
          }
        }

        break;
      }
      case PrimitiveType.QUADS: {
        for (var v = 0; v < pointsCount; v += 4) {
          if (primitive.VertexOrder == VertexOrder.FLIP) {
            yield return v + 1;
            yield return v + 0;
            yield return v + 2;

            yield return v + 3;
            yield return v + 2;
            yield return v + 0;
          } else {
            yield return v + 0;
            yield return v + 1;
            yield return v + 2;

            yield return v + 2;
            yield return v + 3;
            yield return v + 0;
          }
        }

        break;
      }
      default: throw new NotImplementedException();
    }
  }

  public static IEnumerable<(int, int, int)>
      GetOrderedTriangleVertexIndexTriplets(this IReadOnlyPrimitive primitive)
    => primitive.GetOrderedTriangleVertexIndices().SeparateTriplets();

  public static IEnumerable<IReadOnlyVertex> GetOrderedTriangleVertices(
      this IReadOnlyPrimitive primitive)
    => primitive.GetOrderedTriangleVertexIndices()
                .Select(index => primitive.Vertices[index]);
}