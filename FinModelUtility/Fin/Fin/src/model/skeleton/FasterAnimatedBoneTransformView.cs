using System.Numerics;

using fin.animation;
using fin.animation.interpolation;
using fin.data.indexable;

namespace fin.model.skeleton;

public sealed class FasterAnimatedBoneTransformView : IBoneTransformView {
  private readonly IndexableDictionary<IReadOnlyBone,
      (IFastInterpolatable<Vector3>? translations,
      IFastInterpolatable<Quaternion>? rotations,
      IFastInterpolatable<Vector3>? scales)> fastInterpolatablesByBone_ = new();

  private IFastInterpolatable<Vector3>? currentTranslations_;
  private IFastInterpolatable<Quaternion>? currentRotations_;
  private IFastInterpolatable<Vector3>? currentScales_;

  public IReadOnlyModelAnimation? Animation {
    get;
    set {
      field = value;
      this.fastInterpolatablesByBone_.Clear();

      this.currentTranslations_ = null;
      this.currentRotations_ = null;
      this.currentScales_ = null;
    }
  }

  public IAnimationPlaybackManager? PlaybackManager { get; set; }

  private float Frame => (float) (this.PlaybackManager?.Frame ?? 0);

  public void TargetBone(IReadOnlyBone bone) {
    if (this.fastInterpolatablesByBone_.TryGetValue(
            bone,
            out var fastInterpolatables)) {
      (this.currentTranslations_, this.currentRotations_, this.currentScales_)
          = fastInterpolatables;
      return;
    }

    if (this.Animation?.BoneTracks.TryGetValue(bone, out var boneTracks) ??
        false) {
      // Only gets the values from the animation if the frame is at least partially defined.
      if (boneTracks.Translations?.HasAnyData ?? false) {
        this.currentTranslations_
            = FastInterpolatable.From(boneTracks.Translations);
      } else {
        this.currentTranslations_ = null;
      }

      if (boneTracks.Rotations?.HasAnyData ?? false) {
        this.currentRotations_ = FastInterpolatable.From(boneTracks.Rotations);
      } else {
        this.currentRotations_ = null;
      }

      if (boneTracks.Scales?.HasAnyData ?? false) {
        this.currentScales_ = FastInterpolatable.From(boneTracks.Scales);
      } else {
        this.currentScales_ = null;
      }

      this.fastInterpolatablesByBone_[bone] = (
          this.currentTranslations_,
          this.currentRotations_,
          this.currentScales_);
    }
  }

  public bool TryGetWorldTranslation(out Vector3 translation) {
    translation = default;
    return false;
  }

  public bool TryGetWorldRotation(out Quaternion rotation) {
    rotation = default;
    return false;
  }

  public bool TryGetWorldScale(out Vector3 scale) {
    scale = default;
    return false;
  }

  public bool TryGetLocalTranslation(out Vector3 translation) {
    if (this.currentTranslations_ == null) {
      translation = default;
      return false;
    }

    return this.currentTranslations_.TryGetAtFrame(this.Frame, out translation);
  }

  public bool TryGetLocalRotation(out Quaternion rotation) {
    if (this.currentRotations_ == null) {
      rotation = default;
      return false;
    }

    return this.currentRotations_.TryGetAtFrame(this.Frame, out rotation);
  }

  public bool TryGetLocalScale(out Vector3 scale) {
    if (this.currentScales_ == null) {
      scale = default;
      return false;
    }

    return this.currentScales_.TryGetAtFrame(this.Frame, out scale);
  }
}