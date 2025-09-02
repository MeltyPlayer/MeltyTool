﻿using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.animation;
using fin.animation.interpolation;
using fin.data.indexable;
using fin.math.interpolation;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model.accessor;
using fin.ui;

namespace fin.model.util;

public enum BoneWeightTransformType {
  FOR_EXPORT_OR_CPU_PROJECTION,
  FOR_RENDERING,
}

public interface IVertexProjector {
  void ProjectVertexPosition(
      IReadOnlyVertex vertex,
      out Vector3 outPosition);

  void ProjectVertexPositionNormal(
      IReadOnlyNormalVertex vertex,
      out Vector3 outPosition,
      out Vector3 outNormal);

  void ProjectVertexPositionNormalTangent(
      IVertexAccessor vertex,
      out Vector3 outPosition,
      out Vector3 outNormal,
      out Vector4 outTangent);


  void ProjectPosition(IReadOnlyBone bone, ref Vector3 xyz);

  void ProjectNormal(IReadOnlyBone bone, ref Vector3 xyz);
}

public interface IReadOnlyBoneTransformManager : IVertexProjector {
  (IReadOnlyBoneTransformManager, IReadOnlyBone)? Parent { get; }

  IReadOnlyFinMatrix4x4 GetWorldMatrix(IReadOnlyBone bone);
  IReadOnlyFinMatrix4x4 GetWorldMatrix(IReadOnlyBoneWeights bone);

  IReadOnlyFinMatrix4x4 GetInverseBindMatrix(IReadOnlyBone bone);
  IReadOnlyFinMatrix4x4 GetInverseBindMatrix(IReadOnlyBoneWeights boneWeights);

  IReadOnlyFinMatrix4x4 GetLocalToWorldMatrix(IReadOnlyBone bone);
}

public interface IBoneTransformManager : IReadOnlyBoneTransformManager {
  void Clear();

  // TODO: Switch this to a thing that returns a projector instance
  void CalculateStaticMatricesForManualProjection(
      IReadOnlyModel model,
      bool forcePreproject = false);

  void CalculateStaticMatricesForRendering(IReadOnlyModel model);

  void CalculateMatrices(
      IReadOnlyBone rootBone,
      IReadOnlyList<IReadOnlyBoneWeights> boneWeightsList,
      (IReadOnlyModelAnimation, float)? animationAndFrame,
      BoneWeightTransformType boneWeightTransformType
  );
}

public class BoneTransformManager : IBoneTransformManager {
  // TODO: This is going to be slow, can we put this somewhere else for O(1) access?
  private readonly IndexableDictionary<IReadOnlyBone, IFinMatrix4x4>
      bonesToWorldMatrices_ = new();

  private readonly IndexableDictionary<IReadOnlyBone, IReadOnlyFinMatrix4x4>
      bonesToInverseWorldMatrices_ = new();

  private readonly IndexableDictionary<IReadOnlyBoneWeights, IFinMatrix4x4>
      boneWeightsToWorldMatrices_ = new();

  private readonly IndexableDictionary<IReadOnlyBoneWeights, IFinMatrix4x4>
      boneWeightsInverseMatrices_ = new();

  private IndexableDictionary<IReadOnlyVertex, IReadOnlyFinMatrix4x4?>
      verticesToWorldMatrices_ = new();

  public (IReadOnlyBoneTransformManager, IReadOnlyBone)? Parent { get; }
  public IReadOnlyFinMatrix4x4 ManagerMatrix { get; }

  public BoneTransformManager() {
    this.ManagerMatrix = FinMatrix4x4.IDENTITY;
  }

  public BoneTransformManager(
      (IReadOnlyBoneTransformManager, IReadOnlyBone) parent) {
    this.Parent = parent;
    var (parentManager, parentBone) = this.Parent.Value;
    this.ManagerMatrix = parentManager.GetWorldMatrix(parentBone);
  }

  public void Clear() {
    this.bonesToWorldMatrices_.Clear();
    this.bonesToInverseWorldMatrices_.Clear();
    this.boneWeightsToWorldMatrices_.Clear();
    this.boneWeightsInverseMatrices_.Clear();
    this.verticesToWorldMatrices_.Clear();
  }

  private void
      InitModelVertices_(IReadOnlyModel model, bool forcePreproject = false) {
    var vertices = model.Skin.Vertices;
    this.verticesToWorldMatrices_ =
        new IndexableDictionary<IReadOnlyVertex, IReadOnlyFinMatrix4x4?>(
            vertices.Count);
    foreach (var vertex in vertices) {
      this.verticesToWorldMatrices_[vertex] =
          this.DetermineTransformMatrix_(vertex.BoneWeights, forcePreproject);
    }
  }

  private readonly MagFilterInterpolatable<Vector3>
      translationMagFilterInterpolationTrack_ = new(new Vector3Interpolator()) {
          AnimationInterpolationMagFilter
              = AnimationInterpolationMagFilter.ORIGINAL_FRAME_RATE_LINEAR
      };

  private readonly MagFilterInterpolatable<Quaternion>
      rotationMagFilterInterpolationTrack_ =
          new(new SimpleQuaternionInterpolator()) {
              AnimationInterpolationMagFilter
                  = AnimationInterpolationMagFilter.ORIGINAL_FRAME_RATE_LINEAR
          };

  private readonly MagFilterInterpolatable<Vector3>
      scaleMagFilterInterpolationTrack_ = new(new Vector3Interpolator()) {
          AnimationInterpolationMagFilter
              = AnimationInterpolationMagFilter.ORIGINAL_FRAME_RATE_LINEAR
      };

  public void CalculateStaticMatricesForManualProjection(
      IReadOnlyModel model,
      bool forcePreproject = false) {
    this.CalculateMatrices(
        model.Skeleton.Root,
        model.Skin.BoneWeights,
        null,
        BoneWeightTransformType.FOR_EXPORT_OR_CPU_PROJECTION);
    this.InitModelVertices_(model, forcePreproject);
  }

  public void CalculateStaticMatricesForRendering(IReadOnlyModel model) {
    this.CalculateMatrices(
        model.Skeleton.Root,
        model.Skin.BoneWeights,
        null,
        BoneWeightTransformType.FOR_RENDERING);
    this.InitModelVertices_(model);
  }

  private readonly List<(IReadOnlyBone, FinMatrix4x4, IReadOnlyFinMatrix4x4)>
      boneList_ = new();

  public void CalculateMatrices(
      IReadOnlyBone rootBone,
      IReadOnlyList<IReadOnlyBoneWeights> boneWeightsList,
      (IReadOnlyModelAnimation, float)? animationAndFrame,
      BoneWeightTransformType boneWeightTransformType
  ) {
    var isFirstPass = animationAndFrame == null;

    var animation = animationAndFrame?.Item1;
    var frame = animationAndFrame?.Item2;

    if (isFirstPass) {
      this.boneList_.Clear();

      var boneQueue = new Queue<(IReadOnlyBone, IReadOnlyFinMatrix4x4)>();
      boneQueue.Enqueue((rootBone, this.ManagerMatrix));
      while (boneQueue.Count > 0) {
        var (bone, parentBoneToWorldMatrix) = boneQueue.Dequeue();

        var boneToWorldMatrix = new FinMatrix4x4();
        this.bonesToWorldMatrices_[bone] = boneToWorldMatrix;

        this.boneList_.Add((bone, boneToWorldMatrix, parentBoneToWorldMatrix));

        foreach (var child in bone.Children) {
          boneQueue.Enqueue((child, boneToWorldMatrix));
        }
      }
    }

    foreach (var (bone, boneToWorldMatrix, parentBoneToWorldMatrix) in this
                 .boneList_) {
      boneToWorldMatrix.CopyFrom(parentBoneToWorldMatrix);

      Vector3? animationLocalTranslation = null;
      Quaternion? animationLocalRotation = null;
      Vector3? animationLocalScale = null;

      // The pose of the animation, if available.
      IReadOnlyBoneTracks? boneTracks = null;
      animation?.BoneTracks.TryGetValue(bone, out boneTracks);
      if (boneTracks != null) {
        // Only gets the values from the animation if the frame is at least partially defined.
        if (boneTracks.Translations?.HasAnyData ?? false) {
          this.translationMagFilterInterpolationTrack_.Impl
              = boneTracks.Translations;
          if (this.translationMagFilterInterpolationTrack_
                  .TryGetAtFrame(
                      frame.Value,
                      out var outAnimationLocalTranslation)) {
            animationLocalTranslation = outAnimationLocalTranslation;
          }
        }

        if (boneTracks.Rotations?.HasAnyData ?? false) {
          this.rotationMagFilterInterpolationTrack_.Impl
              = boneTracks.Rotations;
          if (this.rotationMagFilterInterpolationTrack_
                  .TryGetAtFrame(
                      frame.Value,
                      out var outAnimationLocalRotation)) {
            animationLocalRotation = outAnimationLocalRotation;
          }
        }

        if (boneTracks.Scales?.HasAnyData ?? false) {
          this.scaleMagFilterInterpolationTrack_.Impl = boneTracks.Scales;
          if (this.scaleMagFilterInterpolationTrack_.TryGetAtFrame(
                  frame.Value,
                  out var outAnimationLocalScale)) {
            animationLocalScale = outAnimationLocalScale;
          }
        }
      }

      // Uses the animation pose instead of the root pose when available.
      var localTransform = bone.LocalTransform;
      var localTranslation
          = animationLocalTranslation ?? localTransform.Translation;
      var localRotation = animationLocalRotation ?? localTransform.Rotation;
      var localScale = animationLocalScale ?? localTransform.Scale;

      if (bone is { IgnoreParentScale: false, FaceTowardsCamera: false }) {
        var localMatrix = SystemMatrix4x4Util.FromTrs(localTranslation,
          localRotation,
          localScale);
        boneToWorldMatrix.MultiplyInPlace(localMatrix);
      } else {
        boneToWorldMatrix.ApplyTrsWithFancyBoneEffects(bone,
          localTranslation,
          localRotation,
          localScale,
          isFirstPass);
      }

      if (isFirstPass) {
        this.bonesToInverseWorldMatrices_[bone]
            = boneToWorldMatrix.CloneAndInvert();
      }
    }

    if (isFirstPass) {
      foreach (var boneWeights in boneWeightsList) {
        if (!this.boneWeightsInverseMatrices_.TryGetValue(
                boneWeights,
                out var boneWeightInverseMatrix)) {
          this.boneWeightsInverseMatrices_[boneWeights] =
              boneWeightInverseMatrix = new FinMatrix4x4();
        }

        boneWeightInverseMatrix.SetZero();
        foreach (var boneWeight in boneWeights.Weights) {
          var bone = boneWeight.Bone;
          var weight = boneWeight.Weight;

          var inverseMatrix = boneWeight.InverseBindMatrix ??
                              this.bonesToInverseWorldMatrices_[bone];
          boneWeightInverseMatrix.AddInPlace(inverseMatrix.Impl * weight);
        }
      }
    }

    if (boneWeightTransformType ==
        BoneWeightTransformType.FOR_EXPORT_OR_CPU_PROJECTION ||
        isFirstPass) {
      foreach (var boneWeights in boneWeightsList) {
        if (!this.boneWeightsToWorldMatrices_.TryGetValue(
                boneWeights,
                out var boneWeightMatrix)) {
          this.boneWeightsToWorldMatrices_[boneWeights] =
              boneWeightMatrix = new FinMatrix4x4();
        }

        boneWeightMatrix.SetZero();
        foreach (var boneWeight in boneWeights.Weights) {
          var bone = boneWeight.Bone;
          var weight = boneWeight.Weight;

          var inverseMatrix = boneWeight.InverseBindMatrix ??
                              this.bonesToInverseWorldMatrices_[bone];
          boneWeightMatrix.AddInPlace(
              (inverseMatrix.Impl * this.bonesToWorldMatrices_[bone].Impl) *
              weight);
        }
      }
    }
  }

  public IReadOnlyFinMatrix4x4 GetWorldMatrix(IReadOnlyBone bone)
    => this.bonesToWorldMatrices_[bone];

  public IReadOnlyFinMatrix4x4 GetWorldMatrix(IReadOnlyBoneWeights boneWeights)
    => this.boneWeightsToWorldMatrices_[boneWeights];

  public IReadOnlyFinMatrix4x4 GetInverseBindMatrix(IReadOnlyBone bone)
    => this.bonesToInverseWorldMatrices_[bone];

  public IReadOnlyFinMatrix4x4 GetInverseBindMatrix(
      IReadOnlyBoneWeights boneWeights)
    => this.boneWeightsInverseMatrices_[boneWeights];

  public IReadOnlyFinMatrix4x4 GetLocalToWorldMatrix(IReadOnlyBone bone)
    => this.bonesToWorldMatrices_[bone];

  public IReadOnlyFinMatrix4x4? GetTransformMatrix(IReadOnlyVertex vertex)
    => this.verticesToWorldMatrices_[vertex];

  private IReadOnlyFinMatrix4x4? DetermineTransformMatrix_(
      IReadOnlyBoneWeights? boneWeights,
      bool forcePreproject = false) {
    var weights = boneWeights?.Weights;
    var preproject =
        (boneWeights?.VertexSpace != VertexSpace.RELATIVE_TO_WORLD ||
         forcePreproject) &&
        weights?.Count > 0;

    if (!preproject) {
      return null;
    }

    return this.boneWeightsToWorldMatrices_[boneWeights!];
  }

  public void ProjectVertexPosition(
      IReadOnlyVertex vertex,
      out Vector3 outPosition) {
    outPosition = vertex.LocalPosition;

    var finTransformMatrix = this.GetTransformMatrix(vertex);
    if (finTransformMatrix == null) {
      return;
    }

    var transformMatrix = finTransformMatrix.Impl;
    ProjectionUtil.ProjectPosition(transformMatrix, ref outPosition);
  }

  public void ProjectVertexPositionNormal(
      IReadOnlyNormalVertex vertex,
      out Vector3 outPosition,
      out Vector3 outNormal) {
    outPosition = vertex.LocalPosition;
    outNormal = vertex.LocalNormal.GetValueOrDefault();

    var finTransformMatrix = this.GetTransformMatrix(vertex);
    if (finTransformMatrix == null) {
      return;
    }

    var transformMatrix = finTransformMatrix.Impl;
    ProjectionUtil.ProjectPosition(transformMatrix, ref outPosition);
    if (vertex.LocalNormal.HasValue) {
      ProjectionUtil.ProjectNormal(transformMatrix, ref outNormal);
    }
  }

  public void ProjectVertexPositionNormalTangent(
      IVertexAccessor vertex,
      out Vector3 outPosition,
      out Vector3 outNormal,
      out Vector4 outTangent) {
    outPosition = vertex.LocalPosition;

    outNormal = vertex.LocalNormal.GetValueOrDefault();
    outTangent = vertex.LocalTangent.GetValueOrDefault();

    var finTransformMatrix = this.GetTransformMatrix(vertex);
    if (finTransformMatrix == null) {
      return;
    }

    var transformMatrix = finTransformMatrix.Impl;
    ProjectionUtil.ProjectPosition(transformMatrix, ref outPosition);
    if (vertex.LocalNormal.HasValue) {
      ProjectionUtil.ProjectNormal(transformMatrix, ref outNormal);
    }

    if (vertex.LocalTangent.HasValue) {
      ProjectionUtil.ProjectTangent(transformMatrix, ref outTangent);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ProjectPosition(IReadOnlyBone bone, ref Vector3 xyz)
    => ProjectionUtil.ProjectPosition(this.GetWorldMatrix(bone).Impl,
                                      ref xyz);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void UnprojectPosition(IReadOnlyBone bone, ref Vector3 xyz) {
    Matrix4x4.Invert(this.GetWorldMatrix(bone).Impl, out var inverted);
    ProjectionUtil.ProjectPosition(inverted, ref xyz);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ProjectNormal(IReadOnlyBone bone, ref Vector3 xyz)
    => ProjectionUtil.ProjectNormal(this.GetWorldMatrix(bone).Impl, ref xyz);
}

public static class BoneTransformManagerExtensions {
  public static void ApplyTrsWithFancyBoneEffects(
      this IFinMatrix4x4? matrix,
      IReadOnlyBone bone,
      in Vector3 localPosition,
      in Quaternion? localRotation,
      in Vector3? localScale,
      bool isFirstPass) {
    if (matrix == null) {
      return;
    }

    // Applies translation first, so it's affected by parent rotation/scale.
    var localTranslationMatrix =
        SystemMatrix4x4Util.FromTranslation(localPosition);
    matrix.MultiplyInPlace(localTranslationMatrix);

    // Extracts translation/rotation/scale.
    matrix.CopyTranslationInto(out var translationBuffer);
    Quaternion rotationBuffer;
    if (bone.FaceTowardsCamera && !isFirstPass) {
      var camera = Camera.Instance;
      var yawDegrees = camera?.YawDegrees ?? 0;
      var yawRadians = yawDegrees * FinTrig.DEG_2_RAD;
      var yawRotation =
          Quaternion.CreateFromYawPitchRoll(yawRadians, 0, 0);

      rotationBuffer = yawRotation * bone.FaceTowardsCameraAdjustment;
    } else {
      matrix.CopyRotationInto(out rotationBuffer);
    }

    Vector3 scaleBuffer;
    if (bone.IgnoreParentScale) {
      scaleBuffer = new Vector3(1);
    } else {
      matrix.CopyScaleInto(out scaleBuffer);
    }

    // Gets final matrix.
    FinMatrix4x4Util.FromTrs(
        translationBuffer,
        rotationBuffer,
        scaleBuffer,
        matrix);
    matrix.MultiplyInPlace(
        SystemMatrix4x4Util.FromTrs(null,
                                    localRotation,
                                    localScale));
  }
}