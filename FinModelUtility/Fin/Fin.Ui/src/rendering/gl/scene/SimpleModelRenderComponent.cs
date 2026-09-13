using fin.animation;
using fin.config;
using fin.math;
using fin.model;
using fin.model.skeleton;
using fin.model.skin;
using fin.scene;
using fin.scene.components;
using fin.ui.rendering.gl.model;

namespace fin.ui.rendering.gl.scene;

public sealed class SimpleModelRenderComponent : IModelRenderComponent {
  private readonly IReadOnlyMesh[] meshes_;
  private readonly IModelRenderer modelRenderer_;
  private readonly MeshVisibilityDictionary meshVisibility_;
  private bool isBoneSelected_;
  private bool hadOverrides_;

  private bool needsToAlwaysUpdateMatrices_;

  public SimpleModelRenderComponent(IReadOnlyModel model) {
    this.Model = model;

    this.meshes_ = model.Skin.Meshes.ToArray();
    this.meshVisibility_ = new MeshVisibilityDictionary(model);

    this.SimpleBoneTransformView = new();

    this.BoneTransformManager = new BoneTransformManager();
    this.BoneTransformManager.CalculateStaticMatricesForManualProjection(
        this.Model,
        true);

    this.AnimationPlaybackManager = new FrameAdvancer {
        LoopPlayback = true,
    };

    this.Animation =
        this.Model.AnimationManager.Animations.FirstOrDefault();
    this.AnimationPlaybackManager.IsPlaying = true;

    this.TextureTransformManager = new TextureTransformManager();
    this.TextureSwapManager =
        new TextureSwapManager(model.MaterialManager.Textures);

    this.modelRenderer_ =
        new ModelRenderer(model,
                          this.BoneTransformManager,
                          this.TextureTransformManager,
                          this.TextureSwapManager) {
            MeshVisibility = this.meshVisibility_,
        };

    if (SceneTypeService.IsASingleModel) {
      this.SkeletonRenderer
          = new SkeletonRenderer(model, this.BoneTransformManager);
      SelectedBoneService.OnBoneSelected += selectedBone => {
        var isBoneInModel = false;
        if (selectedBone != null) {
          isBoneInModel = model.Skeleton.Bones.Contains(selectedBone);
        }

        this.isBoneSelected_ = isBoneInModel;
        this.SkeletonRenderer.SelectedBone
            = this.isBoneSelected_ ? selectedBone : null;
      };
    }

    this.needsToAlwaysUpdateMatrices_
        = model.Skeleton.Bones.Any(b => b.FaceTowardsCameraType !=
                                        FaceTowardsCameraType.NONE);
  }

  ~SimpleModelRenderComponent() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.modelRenderer_.Dispose();
    this.TextureSwapManager.Dispose();
  }

  public ISkeletonRenderer? SkeletonRenderer { get; }
  public IModelRenderer ModelRenderer => this.modelRenderer_;

  public void TickAnimatables() {
    var model = this.Model;
    var skeleton = model.Skeleton;

    var animation = this.Animation;
    var animationPlaybackManager = this.AnimationPlaybackManager;

    var hasAnyOverrides = this.SimpleBoneTransformView.HasAnyOverrides;
    if (animation != null ||
        this.needsToAlwaysUpdateMatrices_ ||
        // Need to update for one extra frame, if overrides were just cleared.
        this.hadOverrides_ ||
        hasAnyOverrides) {
      animationPlaybackManager.Tick();
      this.BoneTransformManager.CalculateMatrices(
          skeleton.Root,
          model.Skin.BoneWeights,
          this.SimpleBoneTransformView,
          BoneWeightTransformType.FOR_RENDERING,
          GlTransform.ModelMatrix);
    }
    
    this.hadOverrides_ = hasAnyOverrides;

    if (animation != null) {
      var frame = (float) animationPlaybackManager.Frame;
      this.TextureTransformManager.CalculateMatrices(
          model.MaterialManager.Textures,
          (animation, frame));
      this.TextureSwapManager.UpdateCurrentFlipbookSwaps(
          (animation, frame));

      this.meshVisibility_.Reset();
      if (animation.HasAnyMeshTracks) {
        foreach (var meshTracks in animation.MeshTracks) {
          if (!meshTracks.DisplayStates.TryGetAtFrame(
                  frame,
                  out var displayState)) {
            continue;
          }

          this.meshVisibility_[meshTracks.Mesh]
              = displayState is not MeshDisplayState.HIDDEN;
        }
      }
    } else {
      this.TextureTransformManager.CalculateMatrices(
          model.MaterialManager.Textures,
          null);
      this.TextureSwapManager.UpdateCurrentFlipbookSwaps(null);
    }
  }

  public void Render(ISceneNodeInstance _) => this.Render();

  public void Render(bool allowUpdatingState = true) {
    if (allowUpdatingState) {
      this.TickAnimatables();
    }

    this.modelRenderer_.Render();

    if (FinConfig.ShowSkeleton || this.isBoneSelected_) {
      this.SkeletonRenderer?.Render();
    }
  }

  public IReadOnlyModel Model { get; }

  public IBoneTransformManager BoneTransformManager { get; }
  public SimpleBoneTransformView SimpleBoneTransformView { get; }
  public ITextureTransformManager TextureTransformManager { get; }
  public ITextureSwapManager TextureSwapManager { get; }

  public IReadOnlyModelAnimation? Animation {
    get;
    set {
      if (field == value) {
        return;
      }

      field = value;
      this.SimpleBoneTransformView.AnimatedBoneTransformView.Animation = value;

      var apm = this.AnimationPlaybackManager;

      apm.Frame = 0;
      apm.FrameRate = (int) (value?.FrameRate ?? 20);
      apm.TotalFrames = value?.FrameCount ?? 0;
    }
  }

  public IAnimationPlaybackManager AnimationPlaybackManager {
    get;
    set {
      field = value;
      this.SimpleBoneTransformView.AnimatedBoneTransformView.PlaybackManager
          = value;
    }
  }
}