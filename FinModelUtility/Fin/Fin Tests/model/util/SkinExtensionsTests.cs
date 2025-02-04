using System;
using System.Collections.Generic;
using System.Numerics;

using fin.util.asserts;

using NUnit.Framework;

namespace fin.model.util;

public class SkinExtensionsTests {
  private record
      VertexStub(int Index, Vector3 LocalPosition) : IReadOnlyVertex {
    public IReadOnlyBoneWeights? BoneWeights
      => throw new NotImplementedException();
  }

  private class PrimitiveStub : IReadOnlyPrimitive {
    public required PrimitiveType Type { get; init; }
    public required VertexOrder VertexOrder { get; init; }
    public required IReadOnlyList<IReadOnlyVertex> Vertices { get; init; }

    public IReadOnlyMaterial? Material => throw new NotImplementedException();
    public uint InversePriority => throw new NotImplementedException();
  }

  [Test]
  public void TestGetOrderedTrianglesForNormalTriangles() {
    var vertexX = new VertexStub(0, Vector3.UnitX);
    var vertexY = new VertexStub(1, Vector3.UnitY);
    var vertexZ = new VertexStub(2, Vector3.UnitZ);

    var primitive = new PrimitiveStub {
        Type = PrimitiveType.TRIANGLES,
        VertexOrder = VertexOrder.COUNTER_CLOCKWISE,
        Vertices = [
            vertexX,
            vertexY,
            vertexZ,

            vertexY,
            vertexZ,
            vertexX,

            vertexZ,
            vertexY,
            vertexX
        ]
    };

    Asserts.SequenceEqual(
        primitive.GetOrderedTriangleVertexIndexTriplets(),
        [
            (0, 1, 2),
            (3, 4, 5),
            (6, 7, 8),
        ]);
    Asserts.SequenceEqual(
        primitive.GetOrderedTriangleVertices(),
        [
            vertexX,
            vertexY,
            vertexZ,

            vertexY,
            vertexZ,
            vertexX,

            vertexZ,
            vertexY,
            vertexX
        ]);
  }
}