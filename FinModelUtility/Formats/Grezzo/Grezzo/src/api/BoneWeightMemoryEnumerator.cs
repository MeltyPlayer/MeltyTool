using System;
using System.Linq;

using fin.model;
using fin.util.enumerables;

using grezzo.schema.cmb;
using grezzo.schema.cmb.sklm;

namespace grezzo.api;

public class BoneWeightMemoryEnumerator : IMemoryEnumerator<IBoneWeight[]> {
  private readonly Cmb cmb_;
  private readonly Sepd shape_;
  private readonly bool hasBi_;
  private readonly bool hasBw_;
  private readonly IBone[] allFinBones_;

  private readonly int boneCount_;

  private readonly IMemoryEnumerator<PrimitiveSet> primitiveSetEnumerator_;
  private readonly IMemoryEnumerator<float> boneIndexEnumerator_;
  private IBone[]? bones_;

  private readonly IMemoryEnumerator<float> weightEnumerator_;
  private float[]? weights_;

  public BoneWeightMemoryEnumerator(Cmb cmb,
                                    Sepd shape,
                                    bool hasBi,
                                    bool hasBw,
                                    IBone[] allFinBones) {
      this.cmb_ = cmb;
      this.shape_ = shape;
      this.hasBi_ = hasBi;
      this.hasBw_ = hasBw;
      this.allFinBones_ = allFinBones;

      this.boneCount_ = shape.boneDimensions;

      var maxIndex = shape.primitiveSets
                          .SelectMany(p => p.primitive.indices)
                          .Max() +
                     1;
      var primitivesByVertexIndex = new PrimitiveSet[maxIndex];
      foreach (var primitiveSet in shape.primitiveSets) {
        foreach (var vertexIndex in primitiveSet.primitive.indices) {
          primitivesByVertexIndex[(int) vertexIndex] = primitiveSet;
        }
      }

      this.primitiveSetEnumerator_
          = primitivesByVertexIndex.ToMemoryEnumerator();
      this.boneIndexEnumerator_
          = DataTypeUtil
            .Read(cmb.vatr.bIndices, shape.bIndices, this.boneCount_)
            .ToMemoryEnumerator();
      this.weightEnumerator_
          = DataTypeUtil
            .Read(cmb.vatr.bWeights, shape.bWeights, this.boneCount_)
            .ToMemoryEnumerator();
    }

  public IBoneWeight[] Current { get; private set; }

  public bool TryMoveNext(out IBoneWeight[] value) {
      var didUpdateBones = this.TryMoveNextBones(out var bones, out var single);
      if (!this.hasBw_ || single) {
        if ((this.Current?.Length ?? 2) != 1) {
          this.Current = new IBoneWeight[1];
        }

        this.Current[0] = new BoneWeight(bones[0], null, 1);
        value = this.Current;
        return didUpdateBones;
      }

      var didUpdateWeights = this.TryMoveNextWeights(out var weights);
     /*if (!didUpdateWeights && !didUpdateBones) {
        alue = this.Current;
        eturn false;
      *//

      if ((this.Current?.Length ?? -1) ! != this.boneCount_) {
        this.Current = new IBoneWeight[this.boneCount_];
      }

      value = this.Current;

      for (var i = 0; i < this.boneCount_; ++i) {
        this.Current[i] = new BoneWeight(bones[i], null, weights[i]);
      }

      return true;
    }

  public bool TryMoveNextWeights(out float[] weights) {
      var bWeights = this.shape_.bWeights;
      var boneCount = this.shape_.boneDimensions;

      if (bWeights.Mode == VertexAttributeMode.Constant) {
        if (this.weights_ != null) {
          weights = this.weights_;
          return false;
        }

        weights = this.weights_ = bWeights.Constants
                                          .Take(boneCount)
                                          .Select(v => v / 100)
                                          .ToArray();
        return true;
      }

      weights = this.weights_ ??= new float[this.boneCount_];
      return this.weightEnumerator_.TryReadInto(this.weights_);
    }

  public bool TryMoveNextBones(out IBone[] bones, out bool single) {
      var primitiveSet
          = this.primitiveSetEnumerator_.TryMoveNextAndGetCurrent();
      var boneCount = this.shape_.boneDimensions;

      Span<float> readBoneIndices = stackalloc float[boneCount];
      Span<int> boneIndices = stackalloc int[boneCount];

      Span<float> boneEnumeratorIndices = stackalloc float[boneCount];
      this.boneIndexEnumerator_.TryReadInto(boneEnumeratorIndices);

      if (this.hasBi_ && primitiveSet.skinningMode != SkinningMode.Single) {
        single = false;

        if (this.shape_.bIndices.Mode == VertexAttributeMode.Constant) {
          this.shape_.bIndices.Constants.AsSpan(0, boneCount)
              .CopyTo(readBoneIndices);
        } else {
          boneEnumeratorIndices.CopyTo(readBoneIndices);
        }

        for (var i = 0; i < boneCount; ++i) {
          boneIndices[i] = primitiveSet.boneTable[(int) readBoneIndices[i]];
        }
      } else {
        single = true;
        boneIndices.Fill(primitiveSet.boneTable[0]);
      }

      var didUpdate = false;
      if ((this.bones_?.Length ?? -1) != boneIndices.Length) {
        didUpdate = true;
        this.bones_ = new IBone[boneIndices.Length];
      }

      bones = this.bones_;
      for (var i = 0; i < boneCount; ++i) {
        var newBone = this.allFinBones_[boneIndices[i]];
        didUpdate = didUpdate || this.bones_[i] != newBone;
        this.bones_[i] = newBone;
      }

      return didUpdate;
    }
}