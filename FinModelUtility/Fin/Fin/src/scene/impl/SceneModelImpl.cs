using System;
using System.Linq;

using fin.animation;
using fin.data.dictionaries;
using fin.math;
using fin.model;

namespace fin.scene {
  public partial class SceneImpl {
    private class SceneModelImpl : ISceneModel {
      private readonly ListDictionary<IReadOnlyBone, ISceneModel> children_ = new();
      private IReadOnlyModelAnimation? animation_;

      public SceneModelImpl(IReadOnlyModel model) {
        this.Model = model;
        this.BoneTransformManager = new BoneTransformManager();

        this.Init_();
      }

      private SceneModelImpl(IReadOnlyModel model,
                             SceneModelImpl parent,
                             IReadOnlyBone bone) {
        this.Model = model;
        this.BoneTransformManager =
            new BoneTransformManager((parent.BoneTransformManager, bone));

        this.Init_();
      }

      private void Init_() {
        this.BoneTransformManager.CalculateStaticMatricesForManualProjection(
            this.Model,
            true);

        this.AnimationPlaybackManager = new FrameAdvancer {
            LoopPlayback = true,
        };
        this.Animation =
            this.Model.AnimationManager.Animations.FirstOrDefault();
        this.AnimationPlaybackManager.IsPlaying = true;
      }

      ~SceneModelImpl() => this.ReleaseUnmanagedResources_();

      public void Dispose() {
        this.ReleaseUnmanagedResources_();
        GC.SuppressFinalize(this);
      }

      private void ReleaseUnmanagedResources_() {
        foreach (var child in this.children_.Values) {
          child.Dispose();
        }
      }

      public IReadOnlyListDictionary<IReadOnlyBone, ISceneModel> Children
        => this.children_;

      public ISceneModel AddModelOntoBone(IReadOnlyModel model,
                                          IReadOnlyBone bone) {
        var child = new SceneModelImpl(model, this, bone);
        this.children_.Add(bone, child);
        return child;
      }

      public IReadOnlyModel Model { get; }

      public IBoneTransformManager BoneTransformManager { get; }

      public IReadOnlyModelAnimation? Animation {
        get => this.animation_;
        set {
          if (this.animation_ == value) {
            return;
          }

          this.animation_ = value;

          var apm = this.AnimationPlaybackManager;
          apm.Frame = 0;
          apm.FrameRate = (int) (value?.FrameRate ?? 20);
          apm.TotalFrames = value?.FrameCount ?? 0;
          apm.Config = new AnimationInterpolationConfig {
              UseLoopingInterpolation = value?.UseLoopingInterpolation ?? false
          };
        }
      }

      public IAnimationPlaybackManager AnimationPlaybackManager {
        get;
        private set;
      }


      public float ViewerScale { get; set; } = 1;
    }
  }
}