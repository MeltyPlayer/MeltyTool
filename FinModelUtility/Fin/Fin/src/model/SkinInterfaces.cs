using System;
using System.Collections.Generic;
using System.Numerics;

using fin.data.indexable;
using fin.data.sets;
using fin.math.matrix.four;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface ISkin {
  IReadOnlyList<IVertex> Vertices { get; }

  IReadOnlyList<IMesh> Meshes { get; }
  IMesh AddMesh();
  bool AllowMaterialRendererMerging { get; set; }

  IReadOnlyFinSet<IReadOnlyBone> BonesUsedByVertices { get; }
  IReadOnlyList<IBoneWeights> BoneWeights { get; }

  IBoneWeights GetOrCreateBoneWeights(
      VertexSpace vertexSpace,
      IReadOnlyBone bone);

  IBoneWeights GetOrCreateBoneWeights(
      VertexSpace vertexSpace,
      params IBoneWeight[] weights);

  IBoneWeights CreateBoneWeights(
      VertexSpace vertexSpace,
      params IBoneWeight[] weights);
}

[GenerateReadOnly]
public partial interface ISkin<out TVertex> : ISkin
    where TVertex : IReadOnlyVertex {
  IReadOnlyList<TVertex> TypedVertices { get; }
  TVertex AddVertex(Vector3 position);
}

[GenerateReadOnly]
public partial interface IMesh : IIndexable {
  string Name { get; set; }

  IReadOnlyList<IPrimitive> Primitives { get; }

  MeshDisplayState DefaultDisplayState { get; set; }

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

  ILinesPrimitive AddLineStrip(IReadOnlyList<IReadOnlyVertex> lines);
  ILinesPrimitive AddLineStrip(params IReadOnlyVertex[] lines);

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
  IReadOnlyBone Bone { get; }
  IReadOnlyFinMatrix4x4? InverseBindMatrix { get; }
  float Weight { get; }
}

public record BoneWeight(
    IReadOnlyBone Bone,
    // TODO: This should be moved to the bone interface instead.
    IReadOnlyFinMatrix4x4? InverseBindMatrix,
    float Weight) : IBoneWeight {
  public override int GetHashCode() {
    int hash = 216613626;
    var sub = 16780669;

    hash = hash * sub ^ this.Bone.Index.GetHashCode();
    if (this.InverseBindMatrix != null) {
      hash = hash * sub ^ this.InverseBindMatrix.GetHashCode();
    }

    hash = hash * sub ^ this.Weight.GetHashCode();

    return hash;
  }
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
  LINE_STRIP,
  POINTS,
  // TODO: Other types.
}

public enum VertexOrder {
  NORMAL,
  FLIP,
}

[GenerateReadOnly]
public partial interface ILinesPrimitive : IPrimitive {
  float LineWidth { get; }
  ILinesPrimitive SetLineWidth(float width);
}

[GenerateReadOnly]
public partial interface IPointsPrimitive : IPrimitive {
  float Radius { get; }
  IPointsPrimitive SetRadius(float radius);
}

[GenerateReadOnly]
public partial interface IPrimitive {
  PrimitiveType Type { get; }
  IReadOnlyList<IReadOnlyVertex> Vertices { get; }

  IMaterial? Material { get; }
  IPrimitive SetMaterial(IMaterial? material);

  VertexOrder VertexOrder { get; }
  IPrimitive SetVertexOrder(VertexOrder vertexOrder);

  /// <summary>
  ///   Rendering priority when determining what order to draw in. Lower
  ///   values will be prioritized higher.
  /// </summary>
  uint InversePriority { get; }

  IPrimitive SetInversePriority(uint inversePriority);
}