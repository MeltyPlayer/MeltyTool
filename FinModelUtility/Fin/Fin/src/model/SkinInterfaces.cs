using System;
using System.Collections.Generic;
using System.Numerics;

using fin.data.indexable;
using fin.data.sets;
using fin.math.matrix.four;
using fin.math.xyz;

using schema.readOnly;

namespace fin.model {
  public interface ISkin {
    IReadOnlyList<IReadOnlyVertex> Vertices { get; }

    IReadOnlyList<IMesh> Meshes { get; }
    IMesh AddMesh();
    bool AllowMaterialRendererMerging { get; set; }

    IReadOnlyFinSet<IBone> BonesUsedByVertices { get; }
    IReadOnlyList<IBoneWeights> BoneWeights { get; }

    IBoneWeights GetOrCreateBoneWeights(
        VertexSpace vertexSpace,
        IBone bone);

    IBoneWeights GetOrCreateBoneWeights(
        VertexSpace vertexSpace,
        params IBoneWeight[] weights);

    IBoneWeights CreateBoneWeights(
        VertexSpace vertexSpace,
        params IBoneWeight[] weights);
  }

  public interface ISkin<out TVertex> : ISkin where TVertex : IReadOnlyVertex {
    IReadOnlyList<TVertex> TypedVertices { get; }

    TVertex AddVertex(Position position);
    TVertex AddVertex(Vector3 position);
    TVertex AddVertex(IReadOnlyXyz position);
    TVertex AddVertex(float x, float y, float z);
  }

  public interface IMesh {
    string Name { get; set; }

    IReadOnlyList<IPrimitive> Primitives { get; }

    IPrimitive AddTriangles(
        IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)>
            vertices);

    IPrimitive AddTriangles(
        params (IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)[] triangles);

    IPrimitive AddTriangles(IReadOnlyList<IReadOnlyVertex> vertices);
    IPrimitive AddTriangles(params IReadOnlyVertex[] vertices);

    IPrimitive AddTriangleStrip(IReadOnlyList<IReadOnlyVertex> vertices);
    IPrimitive AddTriangleStrip(params IReadOnlyVertex[] vertices);
    IPrimitive AddTriangleFan(IReadOnlyList<IReadOnlyVertex> vertices);
    IPrimitive AddTriangleFan(params IReadOnlyVertex[] vertices);

    IPrimitive AddQuads(
        IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex,
            IReadOnlyVertex)> vertices);

    IPrimitive AddQuads(
        params (IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex,
            IReadOnlyVertex)[] quads);

    IPrimitive AddQuads(IReadOnlyList<IReadOnlyVertex> vertices);
    IPrimitive AddQuads(params IReadOnlyVertex[] vertices);

    ILinesPrimitive AddLines(
        IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex)> lines);

    ILinesPrimitive AddLines(params (IReadOnlyVertex, IReadOnlyVertex)[] lines);
    ILinesPrimitive AddLines(IReadOnlyList<IReadOnlyVertex> lines);
    ILinesPrimitive AddLines(params IReadOnlyVertex[] lines);

    IPointsPrimitive AddPoints(IReadOnlyList<IReadOnlyVertex> points);
    IPointsPrimitive AddPoints(params IReadOnlyVertex[] points);
  }

  [GenerateReadOnly]
  public partial interface IBoneWeights
      : IIndexable, IEquatable<IReadOnlyBoneWeights> {
    VertexSpace VertexSpace { get; }
    IReadOnlyList<IBoneWeight> Weights { get; }

    [Const]
    bool Equals(VertexSpace vertexSpace,
                IReadOnlyList<IReadOnlyBoneWeight> weights);
  }

  [GenerateReadOnly]
  public partial interface IBoneWeight {
    IBone Bone { get; }
    IReadOnlyFinMatrix4x4? InverseBindMatrix { get; }
    float Weight { get; }
  }

  public record BoneWeight(
      IBone Bone,
      // TODO: This should be moved to the bone interface instead.
      IReadOnlyFinMatrix4x4? InverseBindMatrix,
      float Weight) : IBoneWeight {
    public override int GetHashCode() {
      int hash = 216613626;
      var sub = 16780669;

      hash = hash * sub ^ Bone.Index.GetHashCode();
      if (InverseBindMatrix != null) {
        hash = hash * sub ^ InverseBindMatrix.GetHashCode();
      }

      hash = hash * sub ^ Weight.GetHashCode();

      return hash;
    }
  }

  public readonly struct TexCoord {
    public float U { get; init; }
    public float V { get; init; }

    public override string ToString() => $"{{{this.U}, {this.V}}}";
  }

  public enum VertexSpace {
    RELATIVE_TO_WORLD,
    RELATIVE_TO_BONE,
  }


  public enum PrimitiveType {
    TRIANGLES,
    TRIANGLE_STRIP,
    TRIANGLE_FAN,
    QUADS,
    LINES,
    POINTS,
    // TODO: Other types.
  }

  public enum VertexOrder {
    NORMAL,
    FLIP,
  }

  public interface ILinesPrimitive : IPrimitive {
    float LineWidth { get; }
    ILinesPrimitive SetLineWidth(float width);
  }

  public interface IPointsPrimitive : IPrimitive {
    float Radius { get; }
    IPointsPrimitive SetRadius(float radius);
  }

  public interface IPrimitive {
    PrimitiveType Type { get; }
    IReadOnlyList<IReadOnlyVertex> Vertices { get; }

    IEnumerable<int> GetOrderedTriangleVertexIndices();
    IEnumerable<(int, int, int)> GetOrderedTriangleVertexIndexTriplets();
    IEnumerable<IReadOnlyVertex> GetOrderedTriangleVertices();

    IMaterial Material { get; }
    IPrimitive SetMaterial(IMaterial material);

    VertexOrder VertexOrder { get; }
    IPrimitive SetVertexOrder(VertexOrder vertexOrder);

    /// <summary>
    ///   Rendering priority when determining what order to draw in. Lower
    ///   values will be prioritized higher.
    /// </summary>
    uint InversePriority { get; }

    IPrimitive SetInversePriority(uint inversePriority);
  }
}