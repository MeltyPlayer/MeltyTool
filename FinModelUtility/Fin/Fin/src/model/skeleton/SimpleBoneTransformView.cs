using System.Numerics;

using fin.data.indexable;


using fin.model.util;

namespace fin.model.skeleton;

/// <summary>
///   Well, this is simple for downstream users at least.
/// </summary>
public sealed class SimpleBoneTransformView : IBoneTransformView {
  private const bool USE_FASTER_TRANSFORM_VIEW = false;

  private readonly IBoneTransformView[] impls_;

  private IndexableDictionary<IReadOnlyBone, Vector3> worldTranslationOverrides_
      = new();

  private IndexableDictionary<IReadOnlyBone, Quaternion> worldRotationOverrides_
      = new();

  private IndexableDictionary<IReadOnlyBone, Vector3> worldScaleOverrides_
      = new();

  private IReadOnlyBone? bone_;

  public SimpleBoneTransformView() {
    this.AnimatedBoneTransformView = new();
    this.impls_ = [
        this.AnimatedBoneTransformView,
        new RootBoneTransformView(),
    ];
  }

  #if USE_FASTER_TRANSFORM_VIEW
  public FasterAnimatedBoneTransformView AnimatedBoneTransformView { get; }
  #else
  public AnimatedBoneTransformView AnimatedBoneTransformView { get; }
  #endif

  public bool HasAnyOverrides
    => this.worldTranslationOverrides_.Count > 0 ||
       this.worldRotationOverrides_.Count > 0 ||
       this.worldScaleOverrides_.Count > 0;

  public void OverrideWorldTranslation(IReadOnlyBone bone,
                                       Vector3 translation)
    => this.worldTranslationOverrides_[bone] = translation;

  public void OverrideWorldRotation(IReadOnlyBone bone, Quaternion rotation)
    => this.worldRotationOverrides_[bone] = rotation;

  public void OverrideWorldScale(IReadOnlyBone bone, Vector3 scale)
    => this.worldScaleOverrides_[bone] = scale;

  public void ClearWorldTranslationOverride(IReadOnlyBone bone)
    => this.worldTranslationOverrides_.Remove(bone);

  public void ClearWorldRotationOverride(IReadOnlyBone bone)
    => this.worldRotationOverrides_.Remove(bone);

  public void ClearWorldScaleOverride(IReadOnlyBone bone)
    => this.worldScaleOverrides_.Remove(bone);

  public void TargetBone(IReadOnlyBone bone) {
    this.bone_ = bone;
    foreach (var impl in this.impls_) {
      impl.TargetBone(bone);
    }
  }

  public bool TryGetWorldTranslation(out Vector3 translation) {
    if (this.worldTranslationOverrides_.TryGetValue(
            this.bone_,
            out translation)) {
      return true;
    }

    foreach (var impl in this.impls_) {
      if (impl.TryGetWorldTranslation(out translation)) {
        return true;
      }
    }

    translation = default;
    return false;
  }

  public bool TryGetWorldRotation(out Quaternion rotation) {
    if (this.worldRotationOverrides_.TryGetValue(
            this.bone_,
            out rotation)) {
      return true;
    }

    if (this.bone_?.TryGetFaceCameraQuaternion(out rotation) ?? false) {
      return true;
    }

    foreach (var impl in this.impls_) {
      if (impl.TryGetWorldRotation(out rotation)) {
        return true;
      }
    }

    rotation = default;
    return false;
  }

  public bool TryGetWorldScale(out Vector3 scale) {
    if (this.worldScaleOverrides_.TryGetValue(
            this.bone_,
            out scale)) {
      return true;
    }

    foreach (var impl in this.impls_) {
      if (impl.TryGetWorldScale(out scale)) {
        return true;
      }
    }

    scale = default;
    return false;
  }

  public bool TryGetLocalTranslation(out Vector3 translation) {
    foreach (var impl in this.impls_) {
      if (impl.TryGetLocalTranslation(out translation)) {
        return true;
      }
    }

    translation = default;
    return false;
  }

  public bool TryGetLocalRotation(out Quaternion rotation) {
    foreach (var impl in this.impls_) {
      if (impl.TryGetLocalRotation(out rotation)) {
        return true;
      }
    }

    rotation = default;
    return false;
  }

  public bool TryGetLocalScale(out Vector3 scale) {
    foreach (var impl in this.impls_) {
      if (impl.TryGetLocalScale(out scale)) {
        return true;
      }
    }

    scale = default;
    return false;
  }
}