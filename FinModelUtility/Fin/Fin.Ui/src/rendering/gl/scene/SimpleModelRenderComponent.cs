using fin.animation;
using System.Numerics;

using fin.config;
using fin.data.indexable;
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
  private readonly Vector3[] baseVertexPositions_;
  private readonly Vector3?[] baseVertexNormals_;
  private int displayedMorphFrame_ = -1;

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
    this.TextureFlipbookSwapManager =
        new TextureFlipbookSwapManager(model.MaterialManager.Textures);

    var hasMorphAnimations = model.AnimationManager.Animations.Any(
        animation => animation.MorphTargetFrames.Count > 0);
    this.baseVertexPositions_ = model.Skin.Vertices
                                     .Select(vertex => vertex.LocalPosition)
                                     .ToArray();
    this.baseVertexNormals_ = model.Skin.Vertices
                                   .Select(vertex =>
                                               (vertex as IReadOnlyNormalVertex)
                                               ?.LocalNormal)
                                   .ToArray();

    this.modelRenderer_ = hasMorphAnimations
        ? fin.ui.rendering.gl.model.ModelRenderer.CreateDynamic(model,
                                      this.BoneTransformManager,
                                      this.TextureTransformManager,
                                      this.TextureFlipbookSwapManager)
        : fin.ui.rendering.gl.model.ModelRenderer.CreateStatic(model,
                                     this.BoneTransformManager,
                                     this.TextureTransformManager,
                                     this.TextureFlipbookSwapManager);
    this.modelRenderer_.MeshVisibility = this.meshVisibility_;

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
    this.TextureFlipbookSwapManager.Dispose();
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
      this.ApplyMorphFrame_(animation, frame);
      this.TextureTransformManager.CalculateMatrices(
          model.MaterialManager.Textures,
          (animation, frame));
      this.TextureFlipbookSwapManager.UpdateCurrentFlipbookSwaps(
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
      this.TextureFlipbookSwapManager.UpdateCurrentFlipbookSwaps(null);
    }
  }

  private void ApplyMorphFrame_(IReadOnlyModelAnimation animation,
                                float frame) {
    var morphTargetFrames = animation.MorphTargetFrames;
    if (morphTargetFrames.Count == 0) {
      return;
    }

    var frameIndex = Math.Clamp((int) MathF.Floor(frame),
                                0,
                                morphTargetFrames.Count - 1);
    if (frameIndex == this.displayedMorphFrame_) {
      return;
    }

    var morphTarget = morphTargetFrames[frameIndex];
    var vertices = this.Model.Skin.Vertices;
    for (var i = 0; i < vertices.Count; ++i) {
      if (vertices[i] is not IVertex vertex) {
        continue;
      }

      vertex.SetLocalPosition(
          morphTarget != null &&
          morphTarget.PositionMorphs.TryGetValue(vertices[i], out var position)
              ? position
              : this.baseVertexPositions_[i]);

      if (vertex is INormalVertex normalVertex) {
        normalVertex.SetLocalNormal(
            morphTarget != null &&
            morphTarget.NormalMorphs.TryGetValue(vertices[i], out var normal)
                ? normal
                : this.baseVertexNormals_[i]);
      }
    }

    (this.modelRenderer_ as IDynamicModelRenderer)?.UpdateBuffer();

    // The dynamic GL buffer now owns a copy of the pose. Restore the Fin model
    // immediately so exporting while an animation is playing still uses the
    // canonical base mesh rather than whichever frame happened to be visible.
    for (var i = 0; i < vertices.Count; ++i) {
      if (vertices[i] is not IVertex vertex) {
        continue;
      }

      vertex.SetLocalPosition(this.baseVertexPositions_[i]);
      if (vertex is INormalVertex normalVertex) {
        normalVertex.SetLocalNormal(this.baseVertexNormals_[i]);
      }
    }

    this.displayedMorphFrame_ = frameIndex;
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
  public ITextureFlipbookSwapManager TextureFlipbookSwapManager { get; }

  public IReadOnlyModelAnimation? Animation {
    get;
    set {
      if (field == value) {
        return;
      }

      field = value;
      this.displayedMorphFrame_ = -1;
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
