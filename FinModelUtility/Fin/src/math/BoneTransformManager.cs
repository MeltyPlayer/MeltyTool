using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.data;
using fin.math.matrix;
using fin.model;
using fin.ui;

namespace fin.math {
  public interface IBoneTransformManager {
    (IBoneTransformManager, IBone)? Parent { get; }

    void Clear();

    void InitModelVertices(IModel model, bool forcePreproject = false);

    IDictionary<IBone, int> CalculateMatrices(
        IBone rootBone,
        IReadOnlyList<IBoneWeights> boneWeightsList,
        (IAnimation, float)? animationAndFrame,
        bool useLoopingInterpolation = false
    );

    public IReadOnlyFinMatrix4x4 GetLocalMatrix(IBone bone);

    public IReadOnlyFinMatrix4x4 GetWorldMatrix(IBone bone);

    public IReadOnlyFinMatrix4x4? GetTransformMatrix(IVertex vertex);

    void ProjectVertexPosition(
        IVertex vertex,
        out Position outPosition);

    void ProjectVertexPositionNormal(
        IVertex vertex,
        out Position outPosition,
        out Normal outNormal);

    void ProjectVertexPositionNormalTangent(
        IVertex vertex,
        out Position outPosition,
        out Normal outNormal,
        out Tangent outTangent);


    void ProjectPosition(IBone bone, ref Vector3 xyz);

    void ProjectNormal(IBone bone, ref Vector3 xyz);
  }

  public class BoneTransformManager : IBoneTransformManager {
    // TODO: This is going to be slow, can we put this somewhere else for O(1) access?
    private readonly IndexableDictionary<IBone, IFinMatrix4x4>
        bonesToLocalMatrices_ = new();

    private readonly IndexableDictionary<IBone, IFinMatrix4x4>
        bonesToWorldMatrices_ = new();

    private readonly IndexableDictionary<IBone, IReadOnlyFinMatrix4x4>
        bonesToInverseBindMatrices_ = new();

    private readonly IndexableDictionary<IBoneWeights, IFinMatrix4x4>
        boneWeightsToWorldMatrices_ = new();

    private IndexableDictionary<IVertex, IReadOnlyFinMatrix4x4?>
        verticesToWorldMatrices_ = new();

    public (IBoneTransformManager, IBone)? Parent { get; }
    public IReadOnlyFinMatrix4x4 ManagerMatrix { get; }

    public BoneTransformManager() {
      this.ManagerMatrix = FinMatrix4x4.IDENTITY;
    }

    public BoneTransformManager((IBoneTransformManager, IBone) parent) {
      this.Parent = parent;
      var (parentManager, parentBone) = this.Parent.Value;
      this.ManagerMatrix = parentManager.GetWorldMatrix(parentBone);
    }

    public void Clear() {
      this.bonesToLocalMatrices_.Clear();
      this.bonesToWorldMatrices_.Clear();
      this.bonesToInverseBindMatrices_.Clear();
      this.boneWeightsToWorldMatrices_.Clear();
      this.verticesToWorldMatrices_.Clear();
    }

    private readonly float[] defaultPosition_ = new float[3];
    private readonly float[] defaultRotation_ = new float[3];
    private readonly float[] defaultScale_ = new float[3];

    public void InitModelVertices(IModel model, bool forcePreproject = false) {
      var vertices = model.Skin.Vertices;
      this.verticesToWorldMatrices_ =
          new IndexableDictionary<IVertex, IReadOnlyFinMatrix4x4?>(
              vertices.Count);
      foreach (var vertex in vertices) {
        this.verticesToWorldMatrices_[vertex] =
            DetermineTransformMatrix_(vertex, forcePreproject);
      }
    }

    public IDictionary<IBone, int> CalculateMatrices(
        IBone rootBone,
        IReadOnlyList<IBoneWeights> boneWeightsList,
        (IAnimation, float)? animationAndFrame,
        bool useLoopingInterpolation = false
    ) {
      var isFirstPass = animationAndFrame == null;

      var animation = animationAndFrame?.Item1;
      var frame = animationAndFrame?.Item2;

      // TODO: Cache this directly on the bone itself instead.
      var bonesToIndex = new Dictionary<IBone, int>();
      var boneIndex = -1;

      var boneQueue = new Queue<(IBone, IReadOnlyFinMatrix4x4)>();
      boneQueue.Enqueue((rootBone, this.ManagerMatrix));
      while (boneQueue.Count > 0) {
        var (bone, parentMatrix) = boneQueue.Dequeue();

        if (!this.bonesToLocalMatrices_.TryGetValue(
                bone,
                out var localMatrix)) {
          this.bonesToLocalMatrices_[bone] = localMatrix = new FinMatrix4x4();
        }

        if (!this.bonesToWorldMatrices_.TryGetValue(bone, out var matrix)) {
          this.bonesToWorldMatrices_[bone] = matrix = new FinMatrix4x4();
        }

        localMatrix.SetIdentity();
        matrix.CopyFrom(parentMatrix);

        // The root pose of the bone.
        var boneLocalPosition = bone.LocalPosition;
        var boneLocalRotation = bone.LocalRotation != null
            ? QuaternionUtil.Create(bone.LocalRotation)
            : (Quaternion?) null;
        var boneLocalScale = bone.LocalScale;

        Position? animationLocalPosition = null;
        Quaternion? animationLocalRotation = null;
        Scale? animationLocalScale = null;

        // The pose of the animation, if available.
        IBoneTracks? boneTracks = null;
        animation?.BoneTracks.TryGetValue(bone, out boneTracks);
        if (boneTracks != null) {
          // Need to pass in default pose of the bone to fill in for any axes that may be undefined.
          defaultPosition_[0] = boneLocalPosition.X;
          defaultPosition_[1] = boneLocalPosition.Y;
          defaultPosition_[2] = boneLocalPosition.Z;

          this.defaultRotation_[0] = bone.LocalRotation?.XRadians ?? 0;
          this.defaultRotation_[1] = bone.LocalRotation?.YRadians ?? 0;
          this.defaultRotation_[2] = bone.LocalRotation?.ZRadians ?? 0;

          this.defaultScale_[0] = boneLocalScale?.X ?? 0;
          this.defaultScale_[1] = boneLocalScale?.Y ?? 0;
          this.defaultScale_[2] = boneLocalScale?.Z ?? 0;

          // Only gets the values from the animation if the frame is at least partially defined.
          animationLocalPosition =
              boneTracks?.Positions.IsDefined ?? false
                  ? boneTracks?.Positions.GetInterpolatedFrame(
                      (float) frame,
                      defaultPosition_,
                      useLoopingInterpolation)
                  : null;
          animationLocalRotation =
              boneTracks?.Rotations.IsDefined ?? false
                  ? boneTracks?.Rotations.GetInterpolatedFrame(
                      (float) frame,
                      defaultRotation_,
                      useLoopingInterpolation)
                  : null;
          animationLocalScale =
              boneTracks?.Scales.IsDefined ?? false
                  ? boneTracks?.Scales.GetInterpolatedFrame(
                      (float) frame,
                      defaultScale_,
                      useLoopingInterpolation)
                  : null;
        }

        // Uses the animation pose instead of the root pose when available.
        var localPosition = animationLocalPosition ?? boneLocalPosition;
        var localRotation = animationLocalRotation ?? boneLocalRotation;
        var localScale = animationLocalScale ?? boneLocalScale;

        if (!bone.IgnoreParentScale && !bone.FaceTowardsCamera) {
          MatrixTransformUtil.FromTrs(localPosition,
                                      localRotation,
                                      localScale,
                                      localMatrix);
          matrix.MultiplyInPlace(localMatrix);
        } else {
          // Applies translation first, so it's affected by parent rotation/scale.
          var localTranslationMatrix =
              MatrixTransformUtil.FromTranslation(localPosition);
          matrix.MultiplyInPlace(localTranslationMatrix);

          // Extracts translation/rotation/scale.
          matrix.CopyTranslationInto(out var translationBuffer);
          Quaternion rotationBuffer;
          if (bone.FaceTowardsCamera) {
            var camera = Camera.Instance;
            var yaw = camera?.Yaw ?? 0;
            var angle = yaw / 180f * MathF.PI;
            var rotateYaw =
                Quaternion.CreateFromYawPitchRoll(angle, 0, 0);

            rotationBuffer = rotateYaw * bone.FaceTowardsCameraAdjustment;
          } else {
            matrix.CopyRotationInto(out rotationBuffer);
          }

          Scale scaleBuffer;
          if (bone.IgnoreParentScale) {
            scaleBuffer = new Scale(1);
          } else {
            matrix.CopyScaleInto(out scaleBuffer);
          }

          // Creates child matrix.
          MatrixTransformUtil.FromTrs(localPosition,
                                      localRotation,
                                      localScale,
                                      localMatrix);

          // Gets final matrix.
          MatrixTransformUtil.FromTrs(
              translationBuffer,
              rotationBuffer,
              scaleBuffer,
              matrix);
          matrix.MultiplyInPlace(MatrixTransformUtil.FromTrs(null,
                                   localRotation,
                                   localScale));
        }

        if (isFirstPass) {
          this.bonesToInverseBindMatrices_[bone] = matrix.CloneAndInvert();
        }

        bonesToIndex[bone] = boneIndex++;

        foreach (var child in bone.Children) {
          // TODO: Use a pool of matrices to prevent unneeded instantiations.
          boneQueue.Enqueue((child, matrix.Clone()));
        }
      }

      foreach (var boneWeights in boneWeightsList) {
        if (!this.boneWeightsToWorldMatrices_.TryGetValue(
                boneWeights,
                out var boneWeightMatrix)) {
          this.boneWeightsToWorldMatrices_[boneWeights] =
              boneWeightMatrix = new FinMatrix4x4();
        }

        boneWeightMatrix.SetZero();

        var weights = boneWeights.Weights;
        if (weights.Count == 1) {
          var weight = weights[0];
          var bone = weight.Bone;

          var skinToBoneMatrix = weight.SkinToBone ??
                                 this.bonesToInverseBindMatrices_[bone];
          var boneMatrix = this.GetWorldMatrix(bone);

          boneMatrix.MultiplyIntoBuffer(skinToBoneMatrix, boneWeightMatrix);
        } else {
          foreach (var weight in weights) {
            var bone = weight.Bone;

            var skinToBoneMatrix = weight.SkinToBone ??
                                   this.bonesToInverseBindMatrices_[bone];
            var boneMatrix = this.GetWorldMatrix(bone);

            boneMatrix.MultiplyIntoBuffer(skinToBoneMatrix,
                                          this.tempSkinToWorldMatrix_);
            this.tempSkinToWorldMatrix_.MultiplyInPlace(weight.Weight);

            boneWeightMatrix.AddInPlace(this.tempSkinToWorldMatrix_);
          }
        }
      }

      return bonesToIndex;
    }

    private readonly FinMatrix4x4 tempSkinToWorldMatrix_ = new();

    public IReadOnlyFinMatrix4x4 GetLocalMatrix(IBone bone)
      => this.bonesToLocalMatrices_[bone];

    public IReadOnlyFinMatrix4x4 GetWorldMatrix(IBone bone)
      => this.bonesToWorldMatrices_[bone];

    public IReadOnlyFinMatrix4x4? GetTransformMatrix(IVertex vertex)
      => this.verticesToWorldMatrices_[vertex];

    private IReadOnlyFinMatrix4x4? DetermineTransformMatrix_(
        IVertex vertex,
        bool forcePreproject = false) {
      var boneWeights = vertex.BoneWeights;
      var weights = vertex.BoneWeights?.Weights;
      var preproject =
          (boneWeights?.PreprojectMode != PreprojectMode.NONE ||
           forcePreproject) &&
          weights?.Count > 0;

      if (!preproject) {
        return null;
      }

      return boneWeights.PreprojectMode switch {
          // If preproject mode is none, then the vertices are already in the same position as the bones.
          // To calculate the animation, we have to first "undo" the root pose via an inverted matrix. 
          PreprojectMode.NONE => this.boneWeightsToWorldMatrices_[
              vertex.BoneWeights!],
          // If preproject mode is bone, then we need to transform the vertex by one or more bones.
          PreprojectMode.BONE => this.boneWeightsToWorldMatrices_[
              vertex.BoneWeights!],
          // If preproject mode is root, then the vertex needs to be transformed relative to
          // some root bone.
          PreprojectMode.ROOT => this.GetWorldMatrix(weights[0].Bone.Root),
          _                   => throw new ArgumentOutOfRangeException()
      };
    }

    public void ProjectVertexPosition(
        IVertex vertex,
        out Position outPosition) {
      outPosition = vertex.LocalPosition;

      var finTransformMatrix = this.GetTransformMatrix(vertex);
      if (finTransformMatrix == null) {
        return;
      }

      var transformMatrix = finTransformMatrix.Impl;
      GlMatrixUtil.ProjectPosition(transformMatrix, ref outPosition);
    }

    public void ProjectVertexPositionNormal(
        IVertex vertex,
        out Position outPosition,
        out Normal outNormal) {
      outPosition = vertex.LocalPosition;
      outNormal = vertex.LocalNormal.GetValueOrDefault();

      var finTransformMatrix = this.GetTransformMatrix(vertex);
      if (finTransformMatrix == null) {
        return;
      }

      var transformMatrix = finTransformMatrix.Impl;
      GlMatrixUtil.ProjectPosition(transformMatrix, ref outPosition);
      if (vertex.LocalNormal.HasValue) {
        GlMatrixUtil.ProjectNormal(transformMatrix, ref outNormal);
      }
    }

    public void ProjectVertexPositionNormalTangent(
        IVertex vertex,
        out Position outPosition,
        out Normal outNormal,
        out Tangent outTangent) {
      outPosition = vertex.LocalPosition;
      outNormal = vertex.LocalNormal.GetValueOrDefault();
      outTangent = vertex.LocalTangent.GetValueOrDefault();

      var finTransformMatrix = this.GetTransformMatrix(vertex);
      if (finTransformMatrix == null) {
        return;
      }

      var transformMatrix = finTransformMatrix.Impl;
      GlMatrixUtil.ProjectPosition(transformMatrix, ref outPosition);
      if (vertex.LocalNormal.HasValue) {
        GlMatrixUtil.ProjectNormal(transformMatrix, ref outNormal);
      }

      if (vertex.LocalTangent.HasValue) {
        GlMatrixUtil.ProjectTangent(transformMatrix, ref outTangent);
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProjectPosition(IBone bone, ref Position xyz)
      => GlMatrixUtil.ProjectPosition(this.GetWorldMatrix(bone).Impl, ref xyz);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProjectPosition(IBone bone, ref Vector3 xyz)
      => GlMatrixUtil.ProjectPosition(this.GetWorldMatrix(bone).Impl, ref xyz);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProjectNormal(IBone bone, ref Normal xyz)
      => GlMatrixUtil.ProjectNormal(this.GetWorldMatrix(bone).Impl, ref xyz);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProjectNormal(IBone bone, ref Vector3 xyz)
      => GlMatrixUtil.ProjectNormal(this.GetWorldMatrix(bone).Impl, ref xyz);
  }
}