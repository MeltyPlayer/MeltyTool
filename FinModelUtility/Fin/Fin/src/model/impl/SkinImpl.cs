using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

using fin.data;
using fin.data.indexable;
using fin.data.sets;
using fin.math.matrix.four;
using fin.math.xyz;
using fin.util.enumerables;

namespace fin.model.impl {
  public partial class ModelImpl<TVertex> {
    ISkin IModel.Skin => this.Skin;
    public ISkin<TVertex> Skin { get; }

    private class SkinImpl : ISkin<TVertex> {
      private readonly Func<int, Position, TVertex> vertexCreator_;
      private readonly IList<TVertex> vertices_;
      private readonly IList<IMesh> meshes_ = new List<IMesh>();

      private readonly FinSortedSet<IBone> bonesUsedByVertices_
          = new((lhs, rhs) => lhs.Index.CompareTo(rhs.Index));
      private readonly BoneWeightsDictionary boneWeightsDictionary_ = new();

      private readonly IndexableDictionary<IBone, IBoneWeights>
          boneWeightsByBone_ = new();

      public SkinImpl(Func<int, Position, TVertex> vertexCreator)
          : this(0, vertexCreator) { }

      public SkinImpl(int vertexCount,
                      Func<int, Position, TVertex> vertexCreator) {
        this.vertexCreator_ = vertexCreator;

        this.vertices_ = new List<TVertex>();
        this.TypedVertices = new ReadOnlyCollection<TVertex>(this.vertices_);
        this.Vertices =
            new CastListView<TVertex, IVertex>(this.TypedVertices);

        // TODO: Possible to speed this up?
        for (var i = 0; i < vertexCount; ++i) {
          this.vertices_.Add(vertexCreator(i, default));
        }

        this.Meshes = new ReadOnlyCollection<IMesh>(this.meshes_);
      }

      public IReadOnlyList<IVertex> Vertices { get; }
      public IReadOnlyList<TVertex> TypedVertices { get; }

      public TVertex AddVertex(Position position) {
        lock (this.vertices_) {
          var vertex = this.vertexCreator_(this.vertices_.Count, position);
          this.vertices_.Add(vertex);
          return vertex;
        }
      }

      public TVertex AddVertex(Vector3 position)
        => this.AddVertex(new Position(position.X, position.Y, position.Z));

      public TVertex AddVertex(IReadOnlyXyz position)
        => this.AddVertex(new Position(position.X, position.Y, position.Z));

      public TVertex AddVertex(float x, float y, float z)
        => this.AddVertex(new Position(x, y, z));


      public IReadOnlyList<IMesh> Meshes { get; }

      public IMesh AddMesh() {
        var mesh = new MeshImpl();
        this.meshes_.Add(mesh);
        return mesh;
      }

      public bool AllowMaterialRendererMerging { get; set; } = true;

      public IReadOnlyFinSet<IBone> BonesUsedByVertices
        => this.bonesUsedByVertices_;

      public IReadOnlyList<IBoneWeights> BoneWeights
        => this.boneWeightsDictionary_.List;

      public IBoneWeights GetOrCreateBoneWeights(
          VertexSpace vertexSpace,
          IBone bone) {
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
                                            params IBoneWeight[] weights)
        => this.boneWeightsDictionary_.Create(vertexSpace, weights);


      private class MeshImpl : IMesh {
        private readonly IList<IPrimitive> primitives_ = new List<IPrimitive>();

        public MeshImpl() {
          this.Primitives =
              new ReadOnlyCollection<IPrimitive>(this.primitives_);
        }

        public string Name { get; set; }

        public IReadOnlyList<IPrimitive> Primitives { get; }

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
          var primitive = new LinesPrimitiveImpl(lines);
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

      private class PrimitiveImpl : BPrimitiveImpl {
        public PrimitiveImpl(PrimitiveType type,
                             IReadOnlyList<IReadOnlyVertex> vertices) :
            base(type, vertices) { }
      }

      private class LinesPrimitiveImpl : BPrimitiveImpl, ILinesPrimitive {
        public LinesPrimitiveImpl(IReadOnlyList<IReadOnlyVertex> vertices) :
            base(PrimitiveType.LINES, vertices) { }

        public float LineWidth { get; private set; }

        public ILinesPrimitive SetLineWidth(float width) {
          this.LineWidth = width;
          return this;
        }
      }

      private class PointsPrimitiveImpl : BPrimitiveImpl, IPointsPrimitive {
        public PointsPrimitiveImpl(IReadOnlyList<IReadOnlyVertex> vertices) :
            base(PrimitiveType.POINTS, vertices) { }

        public float Radius { get; private set; }

        public IPointsPrimitive SetRadius(float radius) {
          this.Radius = radius;
          return this;
        }
      }

      private abstract class BPrimitiveImpl : IPrimitive {
        public BPrimitiveImpl(PrimitiveType type,
                              IReadOnlyList<IReadOnlyVertex> vertices) {
          this.Type = type;
          this.Vertices = vertices;
        }

        public PrimitiveType Type { get; }
        public IReadOnlyList<IReadOnlyVertex> Vertices { get; }


        public IEnumerable<int> GetOrderedTriangleVertexIndices() {
          var pointsCount = Vertices.Count;
          switch (Type) {
            case PrimitiveType.TRIANGLES: {
              for (var v = 0; v < pointsCount; v += 3) {
                if (VertexOrder == VertexOrder.FLIP) {
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

                if (VertexOrder == VertexOrder.FLIP) {
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

                if (VertexOrder == VertexOrder.FLIP) {
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
                if (VertexOrder == VertexOrder.FLIP) {
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

        public IEnumerable<(int, int, int)>
            GetOrderedTriangleVertexIndexTriplets()
          => this.GetOrderedTriangleVertexIndices().SeparateTriplets();

        public IEnumerable<IReadOnlyVertex> GetOrderedTriangleVertices()
          => this.GetOrderedTriangleVertexIndices()
                 .Select(index => this.Vertices[index]);


        public IMaterial Material { get; private set; }

        public IPrimitive SetMaterial(IMaterial material) {
          this.Material = material;
          return this;
        }

        public VertexOrder VertexOrder { get; private set; } = VertexOrder.FLIP;

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
}