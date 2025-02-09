using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

using fin.data.indexable;
using fin.data.sets;
using fin.math.matrix.four;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  ISkin IModel.Skin => this.Skin;
  public ISkin<TVertex> Skin { get; }

  private class SkinImpl : ISkin<TVertex> {
    private readonly Func<int, Vector3, TVertex> vertexCreator_;
    private readonly List<IVertex> vertices_;
    private readonly List<TVertex> typedVertices_;
    private readonly List<IMesh> meshes_ = new();

    private readonly FinSortedSet<IReadOnlyBone> bonesUsedByVertices_
        = new((lhs, rhs) => lhs.Index.CompareTo(rhs.Index));

    private readonly BoneWeightsDictionary boneWeightsDictionary_ = new();

    private readonly IndexableDictionary<IReadOnlyBone, IBoneWeights>
        boneWeightsByBone_ = new();

    public SkinImpl(Func<int, Vector3, TVertex> vertexCreator)
        : this(0, vertexCreator) { }

    public SkinImpl(int vertexCount,
                    Func<int, Vector3, TVertex> vertexCreator) {
      this.vertexCreator_ = vertexCreator;

      this.vertices_ = new List<IVertex>(vertexCount);
      this.typedVertices_ = new List<TVertex>(vertexCount);

      // TODO: Possible to speed this up?
      for (var i = 0; i < vertexCount; ++i) {
        this.AddVertex(default);
      }
    }

    public IReadOnlyList<IVertex> Vertices => this.vertices_;
    public IReadOnlyList<TVertex> TypedVertices => this.typedVertices_;

    public TVertex AddVertex(Vector3 position) {
      lock (this.typedVertices_) {
        lock (this.vertices_) {
          var vertex = this.vertexCreator_(this.vertices_.Count, position);
          this.vertices_.Add(vertex);
          this.typedVertices_.Add(vertex);
          return vertex;
        }
      }
    }

    public IReadOnlyList<IMesh> Meshes => this.meshes_;

    public IMesh AddMesh() {
      var mesh = new MeshImpl(this.meshes_.Count);
      this.meshes_.Add(mesh);
      return mesh;
    }

    public bool AllowMaterialRendererMerging { get; set; } = true;

    public IReadOnlyFinSet<IReadOnlyBone> BonesUsedByVertices
      => this.bonesUsedByVertices_;

    public IReadOnlyList<IBoneWeights> BoneWeights
      => this.boneWeightsDictionary_.List;

    public IBoneWeights GetOrCreateBoneWeights(
        VertexSpace vertexSpace,
        IReadOnlyBone bone) {
      if (!this.boneWeightsByBone_.TryGetValue(bone, out var boneWeights)) {
        boneWeights = this.CreateBoneWeights(
            vertexSpace,
            new BoneWeight(bone, FinMatrix4x4.IDENTITY, 1));
        this.boneWeightsByBone_[bone] = boneWeights;
        this.bonesUsedByVertices_.Add(bone);
      }

      return boneWeights;
    }

    public IBoneWeights GetOrCreateBoneWeights(VertexSpace vertexSpace,
                                               params IBoneWeight[] weights) {
      var boneWeights
          = this.boneWeightsDictionary_.GetOrCreate(
              vertexSpace,
              out var newlyCreated,
              weights);
      if (newlyCreated) {
        foreach (var boneWeight in weights) {
          this.bonesUsedByVertices_.Add(boneWeight.Bone);
        }
      }

      return boneWeights;
    }

    public IBoneWeights CreateBoneWeights(VertexSpace vertexSpace,
                                          params IBoneWeight[] weights) {
      foreach (var boneWeight in weights) {
        this.bonesUsedByVertices_.Add(boneWeight.Bone);
      }

      return this.boneWeightsDictionary_.Create(vertexSpace, weights);
    }


    private class MeshImpl(int index) : IMesh {
      private readonly List<IPrimitive> primitives_ = new();

      public int Index => index;

      public string Name { get; set; }

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

      public ILinesPrimitive AddLineStrip(IReadOnlyList<IReadOnlyVertex> lines) {
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

    private class PrimitiveImpl(
        PrimitiveType type,
        IReadOnlyList<IReadOnlyVertex> vertices)
        : BPrimitiveImpl(type, vertices);

    private class LinesPrimitiveImpl(
        PrimitiveType primitiveType,
        IReadOnlyList<IReadOnlyVertex> vertices)
        : BPrimitiveImpl(primitiveType, vertices), ILinesPrimitive {
      public float LineWidth { get; private set; }

      public ILinesPrimitive SetLineWidth(float width) {
        this.LineWidth = width;
        return this;
      }
    }

    private class PointsPrimitiveImpl(IReadOnlyList<IReadOnlyVertex> vertices)
        : BPrimitiveImpl(PrimitiveType.POINTS, vertices), IPointsPrimitive {
      public float Radius { get; private set; }

      public IPointsPrimitive SetRadius(float radius) {
        this.Radius = radius;
        return this;
      }
    }

    private abstract class BPrimitiveImpl(
        PrimitiveType type,
        IReadOnlyList<IReadOnlyVertex> vertices)
        : IPrimitive {
      public PrimitiveType Type { get; } = type;
      public IReadOnlyList<IReadOnlyVertex> Vertices { get; } = vertices;

      public IReadOnlyMaterial Material { get; private set; }

      public IPrimitive SetMaterial(IReadOnlyMaterial material) {
        this.Material = material;
        return this;
      }

      public VertexOrder VertexOrder { get; private set; } = VertexOrder.CLOCKWISE;

      public IPrimitive SetVertexOrder(VertexOrder vertexOrder) {
        this.VertexOrder = vertexOrder;
        return this;
      }

      public uint InversePriority { get; private set; }

      public IPrimitive SetInversePriority(uint inversePriority) {
        this.InversePriority = inversePriority;
        return this;
      }
    }
  }
}