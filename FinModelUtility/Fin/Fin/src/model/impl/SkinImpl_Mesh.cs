using System.Collections.Generic;
using System.Diagnostics;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class SkinImpl {
    public IReadOnlyList<IMesh> Meshes => this.meshes_;

    public IMesh AddMesh() => this.AddMeshImpl_(false);

    private IMesh AddMeshImpl_(bool isSubMesh) {
      var mesh = new MeshImpl(this, this.meshes_.Count, isSubMesh);
      this.meshes_.Add(mesh);
      return mesh;
    }

    private class MeshImpl(SkinImpl skin, int index, bool isSubMesh) : IMesh {
      private readonly List<IMesh> subMeshes_ = new();
      private readonly List<IPrimitive> primitives_ = new();

      public int Index => index;

      public string Name { get; set; }

      public bool IsSubMesh => isSubMesh;
      public IReadOnlyList<IMesh> SubMeshes => this.subMeshes_;

      public IMesh AddSubMesh() {
        var subMesh = skin.AddMeshImpl_(true);
        this.subMeshes_.Add(subMesh);
        return subMesh;
      }

      public IReadOnlyList<IPrimitive> Primitives => this.primitives_;

      public MeshDisplayState DefaultDisplayState { get; set; }
        = MeshDisplayState.VISIBLE;

      public IPrimitive AddTriangles(
          params (IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)[]
              triangles)
        => this.AddTriangles(
            triangles as IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex,
                IReadOnlyVertex)>);

      public IPrimitive AddTriangles(
          IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)>
              triangles) {
        var vertices = new IReadOnlyVertex[3 * triangles.Count];
        for (var i = 0; i < triangles.Count; ++i) {
          var triangle = triangles[i];
          vertices[3 * i] = triangle.Item1;
          vertices[3 * i + 1] = triangle.Item2;
          vertices[3 * i + 2] = triangle.Item3;
        }

        return this.AddTriangles(vertices);
      }


      public IPrimitive AddTriangles(params IReadOnlyVertex[] vertices)
        => this.AddTriangles(vertices as IReadOnlyList<IReadOnlyVertex>);

      public IPrimitive
          AddTriangles(IReadOnlyList<IReadOnlyVertex> vertices) {
        Debug.Assert(vertices.Count % 3 == 0);
        var primitive = new PrimitiveImpl(PrimitiveType.TRIANGLES, vertices);
        this.primitives_.Add(primitive);
        return primitive;
      }


      public IPrimitive AddTriangleStrip(params IReadOnlyVertex[] vertices)
        => this.AddTriangleStrip(vertices as IReadOnlyList<IReadOnlyVertex>);

      public IPrimitive AddTriangleStrip(
          IReadOnlyList<IReadOnlyVertex> vertices) {
        var primitive =
            new PrimitiveImpl(PrimitiveType.TRIANGLE_STRIP, vertices);
        this.primitives_.Add(primitive);
        return primitive;
      }


      public IPrimitive AddTriangleFan(params IReadOnlyVertex[] vertices)
        => this.AddTriangleFan(vertices as IReadOnlyList<IReadOnlyVertex>);

      public IPrimitive AddTriangleFan(
          IReadOnlyList<IReadOnlyVertex> vertices) {
        var primitive =
            new PrimitiveImpl(PrimitiveType.TRIANGLE_FAN, vertices);
        this.primitives_.Add(primitive);
        return primitive;
      }


      public IPrimitive AddQuads(
          params (IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex,
              IReadOnlyVertex)[] quads)
        => this.AddQuads(
            quads as IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex,
                IReadOnlyVertex, IReadOnlyVertex)>);

      public IPrimitive AddQuads(
          IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex,
              IReadOnlyVertex)> quads) {
        var vertices = new IReadOnlyVertex[4 * quads.Count];
        for (var i = 0; i < quads.Count; ++i) {
          var quad = quads[i];
          vertices[4 * i] = quad.Item1;
          vertices[4 * i + 1] = quad.Item2;
          vertices[4 * i + 2] = quad.Item3;
          vertices[4 * i + 3] = quad.Item4;
        }

        return this.AddQuads(vertices);
      }


      public IPrimitive AddQuads(params IReadOnlyVertex[] vertices)
        => this.AddQuads(vertices as IReadOnlyList<IReadOnlyVertex>);

      public IPrimitive AddQuads(IReadOnlyList<IReadOnlyVertex> vertices) {
        Debug.Assert(vertices.Count % 4 == 0);
        var primitive = new PrimitiveImpl(PrimitiveType.QUADS, vertices);
        this.primitives_.Add(primitive);
        return primitive;
      }


      public IPrimitive AddQuadStrip(IReadOnlyList<IReadOnlyVertex> vertices) {
        var primitive = new PrimitiveImpl(PrimitiveType.QUAD_STRIP, vertices);
        this.primitives_.Add(primitive);
        return primitive;
      }


      public ILinesPrimitive AddLines(
          params (IReadOnlyVertex, IReadOnlyVertex)[] lines)
        => this.AddLines(
            lines as IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex)>);

      public ILinesPrimitive AddLines(
          IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex)> lines) {
        var vertices = new IReadOnlyVertex[2 * lines.Count];
        for (var i = 0; i < lines.Count; ++i) {
          var line = lines[i];
          vertices[2 * i] = line.Item1;
          vertices[2 * i + 1] = line.Item2;
        }

        return this.AddLines(vertices);
      }

      public ILinesPrimitive AddLines(params IReadOnlyVertex[] lines)
        => this.AddLines(lines as IReadOnlyList<IReadOnlyVertex>);

      public ILinesPrimitive AddLines(IReadOnlyList<IReadOnlyVertex> lines) {
        Debug.Assert(lines.Count % 2 == 0);
        var primitive = new LinesPrimitiveImpl(PrimitiveType.LINES, lines);
        this.primitives_.Add(primitive);
        return primitive;
      }


      public ILinesPrimitive AddLineStrip(params IReadOnlyVertex[] lines)
        => this.AddLineStrip(lines as IReadOnlyList<IReadOnlyVertex>);

      public ILinesPrimitive
          AddLineStrip(IReadOnlyList<IReadOnlyVertex> lines) {
        Debug.Assert(lines.Count >= 2);
        var primitive = new LinesPrimitiveImpl(PrimitiveType.LINE_STRIP, lines);
        this.primitives_.Add(primitive);
        return primitive;
      }


      public IPointsPrimitive AddPoints(params IReadOnlyVertex[] points)
        => this.AddPoints(points as IReadOnlyList<IReadOnlyVertex>);

      public IPointsPrimitive AddPoints(
          IReadOnlyList<IReadOnlyVertex> points) {
        var primitive = new PointsPrimitiveImpl(points);
        this.primitives_.Add(primitive);
        return primitive;
      }
    }
  }
}