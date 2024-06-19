using System;
using System.Linq;

using fin.animation;
using fin.data.dictionaries;
using fin.math;
using fin.model;

namespace fin.scene.instance {
  public partial class SceneInstanceImpl {
    private class SceneModelInstanceImpl : ISceneModelInstance {
      private readonly ListDictionary<IReadOnlyBone, ISceneModelInstance>
          children_ = new();

      private IReadOnlyModelAnimation? animation_;

      public SceneModelInstanceImpl(IReadOnlySceneModel model) {
        this.Definition = model;
        this.BoneTransformManager = new BoneTransformManager();

        this.Init_(model);
      }

      private SceneModelInstanceImpl(IReadOnlySceneModel model,
                                     SceneModelInstanceImpl parent,
                                     IReadOnlyBone bone) {
        this.Definition = model;
        this.BoneTransformManager =
            new BoneTransformManager((parent.BoneTransformManager, bone));

        this.Init_(model);
      }

      private void Init_(IReadOnlySceneModel model) {
        foreach (var (bone, children) in model.Children.GetPairs()) {
          foreach (var child in children) {
            this.children_.Add(bone,
                               new SceneModelInstanceImpl(child, this, bone));
          }
        }

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

      ~SceneModelInstanceImpl() => this.ReleaseUnmanagedResources_();

      public void Dispose() {
        this.ReleaseUnmanagedResources_();
        GC.SuppressFinalize(this);
      }

      private void ReleaseUnmanagedResources_() {
        foreach (var child in this.children_.Values) {
          child.Dispose();
        }
      }

      public void Tick() {
        this.AnimationPlaybackManager.Tick();
      }

      public IReadOnlySceneModel Definition { get; }


      public IReadOnlyListDictionary<IReadOnlyBone, ISceneModelInstance>
          Children => this.children_;

      public IReadOnlyModel Model => this.Definition.Model;

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