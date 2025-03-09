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

  private partial class SkinImpl : ISkin<TVertex> {
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

    public TVertex AddVertex(in Vector3 position) {
      lock (this.typedVertices_) {
        lock (this.vertices_) {
          var vertex = this.vertexCreator_(this.vertices_.Count, position);
          this.vertices_.Add(vertex);
          this.typedVertices_.Add(vertex);
          return vertex;
        }
      }
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

    public IBoneWeights GetOrCreateBoneWeights(
        VertexSpace vertexSpace,
        params IReadOnlyBoneWeight[] weights) {
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

    public IBoneWeights CreateBoneWeights(
        VertexSpace vertexSpace,
        params IReadOnlyBoneWeight[] weights) {
      foreach (var boneWeight in weights) {
        this.bonesUsedByVertices_.Add(boneWeight.Bone);
      }

      return this.boneWeightsDictionary_.Create(vertexSpace, weights);
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

      public IReadOnlyMaterial? Material { get; private set; }

      public IPrimitive SetMaterial(IReadOnlyMaterial? material) {
        this.Material = material;
        return this;
      }

      public VertexOrder VertexOrder { get; private set; }
        = VertexOrder.CLOCKWISE;

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