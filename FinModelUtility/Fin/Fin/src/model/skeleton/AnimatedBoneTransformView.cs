using System.Collections.Generic;
using System.Numerics;

using fin.animation;
using fin.animation.interpolation;
using fin.data.indexable;
using fin.math.interpolation;

namespace fin.model.skeleton;

public sealed class AnimatedBoneTransformView : ITargetedBoneTransformView {
  private readonly IndexableArray<IReadOnlyBone, IBoneTransformView>
      boneTransformViewByBone_;

  private IBoneTransformView targetedBoneTransformView_
      = NoopBoneTransformView.Instance;

  public IReadOnlyModelAnimation? Animation {
    set {
      this.targetedBoneTransformView_ = NoopBoneTransformView.Instance;

      for (var i = 0; i < this.boneTransformViewByBone_.Count; ++i) {
        this.boneTransformViewByBone_[i] = NoopBoneTransformView.Instance;
      }

      if (value == null) {
        return;
      }

      foreach (var boneTracks in value.BoneTracks) {
        this.boneTransformViewByBone_[boneTracks.Bone]
            = new SingleBoneTransformView(boneTracks) {
                PlaybackManager = this.PlaybackManager
            };
      }
    }
  }

  public IAnimationPlaybackManager? PlaybackManager {
    get;
    set {
      field = value;
      foreach (var boneTransformView in this.boneTransformViewByBone_) {
        if (boneTransformView is SingleBoneTransformView
            singleBoneTransformView) {
          singleBoneTransformView.PlaybackManager = value;
        }
      }
    }
  }

  public AnimatedBoneTransformView(IReadOnlyList<IReadOnlyBone> bones) {
    this.boneTransformViewByBone_
        = new IndexableArray<IReadOnlyBone, IBoneTransformView>(bones.Count);
  }

  public void TargetBone(IReadOnlyBone bone) {
    this.targetedBoneTransformView_ = this.boneTransformViewByBone_[bone];
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

  public bool TryGetLocalTranslation(out Vector3 translation)
    => this.targetedBoneTransformView_.TryGetLocalTranslation(out translation);

  public bool TryGetLocalRotation(out Quaternion rotation)
    => this.targetedBoneTransformView_.TryGetLocalRotation(out rotation);

  public bool TryGetLocalScale(out Vector3 scale)
    => this.targetedBoneTransformView_.TryGetLocalScale(out scale);

  private class SingleBoneTransformView : IBoneTransformView {
    private readonly MagFilterInterpolatable<Vector3>?
        translationMagFilterInterpolationTrack_;

    private readonly MagFilterInterpolatable<Quaternion>?
        rotationMagFilterInterpolationTrack_;

    private readonly MagFilterInterpolatable<Vector3>?
        scaleMagFilterInterpolationTrack_;

    public IAnimationPlaybackManager? PlaybackManager { get; set; }
    private float Frame => (float) (this.PlaybackManager?.Frame ?? 0);

    public SingleBoneTransformView(IReadOnlyBoneTracks boneTracks) {
      // Only gets the values from the animation if the frame is at least partially defined.
      if (boneTracks.Translations?.HasAnyData ?? false) {
        this.translationMagFilterInterpolationTrack_
            = new MagFilterInterpolatable<Vector3>(new Vector3Interpolator()) {
                AnimationInterpolationMagFilter
                    = AnimationInterpolationMagFilter
                        .ORIGINAL_FRAME_RATE_LINEAR,
                Impl = boneTracks.Translations,
            };
      }

      if (boneTracks.Rotations?.HasAnyData ?? false) {
        this.rotationMagFilterInterpolationTrack_
            = new MagFilterInterpolatable<Quaternion>(
                new SimpleQuaternionInterpolator()) {
                AnimationInterpolationMagFilter
                    = AnimationInterpolationMagFilter
                        .ORIGINAL_FRAME_RATE_LINEAR,
                Impl = boneTracks.Rotations,
            };
      }

      if (boneTracks.Scales?.HasAnyData ?? false) {
        this.scaleMagFilterInterpolationTrack_
            = new MagFilterInterpolatable<Vector3>(
                new Vector3Interpolator()) {
                AnimationInterpolationMagFilter
                    = AnimationInterpolationMagFilter
                        .ORIGINAL_FRAME_RATE_LINEAR,
                Impl = boneTracks.Scales,
            };
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
      if (this.translationMagFilterInterpolationTrack_ == null) {
        translation = default;
        return false;
      }

      return this.translationMagFilterInterpolationTrack_
                 .TryGetAtFrame(this.Frame, out translation);
    }

    public bool TryGetLocalRotation(out Quaternion rotation) {
      if (this.rotationMagFilterInterpolationTrack_ == null) {
        rotation = default;
        return false;
      }

      return this.rotationMagFilterInterpolationTrack_
                 .TryGetAtFrame(this.Frame, out rotation);
    }

    public bool TryGetLocalScale(out Vector3 scale) {
      if (this.scaleMagFilterInterpolationTrack_ == null) {
        scale = default;
        return false;
      }

      return this.scaleMagFilterInterpolationTrack_
                 .TryGetAtFrame(this.Frame, out scale);
    }
  }

  private class NoopBoneTransformView : IBoneTransformView {
    public static NoopBoneTransformView Instance { get; } = new();

    private NoopBoneTransformView() { }

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
      translation = default;
      return false;
    }

    public bool TryGetLocalRotation(out Quaternion rotation) {
      rotation = default;
      return false;
    }

    public bool TryGetLocalScale(out Vector3 scale) {
      scale = default;
      return false;
    }
  }
}