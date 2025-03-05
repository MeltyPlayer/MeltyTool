using System.Numerics;

using fin.config;
using fin.data.dictionaries;
using fin.math;
using fin.math.matrix.four;
using fin.model;
using fin.model.util;
using fin.scene;
using fin.ui.rendering.gl.model;

namespace fin.ui.rendering.gl.scene;

public class SceneModelRenderer : IRenderable, IDisposable {
  private readonly ISceneModelInstance sceneModel_;
  private readonly IReadOnlyMesh[] meshes_;
  private readonly IModelRenderer modelRenderer_;
  private readonly HashSet<IReadOnlyMesh> hiddenMeshes_ = new();
  private bool isBoneSelected_;

  private readonly List<(IReadOnlyBone, SceneModelRenderer[])>
      children_ = new();

  public SceneModelRenderer(ISceneModelInstance sceneModel,
                            IReadOnlyLighting? lighting) {
    this.sceneModel_ = sceneModel;
    this.meshes_ = sceneModel.Model.Skin.Meshes.ToArray();

    var model = sceneModel.Model;
    this.modelRenderer_ =
        new ModelRendererV2(model,
                            lighting,
                            sceneModel.BoneTransformManager,
                            sceneModel.TextureTransformManager) {
            HiddenMeshes = this.hiddenMeshes_,
            UseLighting = new UseLightingDetector().ShouldUseLightingFor(model)
        };

    this.SkeletonRenderer =
        new SkeletonRenderer(model.Skeleton,
                             this.sceneModel_.BoneTransformManager) {
            Scale = this.sceneModel_.ViewerScale
        };

    SelectedBoneService.OnBoneSelected += selectedBone => {
      var isBoneInModel = false;
      if (selectedBone != null) {
        isBoneInModel = model.Skeleton.Bones.Contains(selectedBone);
      }

      this.isBoneSelected_ = isBoneInModel;
      this.SkeletonRenderer.SelectedBone
          = this.isBoneSelected_ ? selectedBone : null;
    };

    foreach (var (bone, boneChildren) in sceneModel.Children.GetPairs()) {
      this.children_.Add(
          (bone,
           boneChildren.Select(child => new SceneModelRenderer(child, lighting))
                       .ToArray()));
    }
  }

  ~SceneModelRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.modelRenderer_.Dispose();
    foreach (var (_, children) in this.children_) {
      foreach (var child in children) {
        child.Dispose();
      }
    }
  }

  public ISkeletonRenderer SkeletonRenderer { get; }

  public void Render() {
    GlTransform.PushMatrix();

    var model = this.sceneModel_.Model;
    var skeleton = model.Skeleton;

    var rootBone = skeleton.Root;
    if (rootBone.FaceTowardsCamera) {
      var camera = Camera.Instance;
      var angle = camera.YawDegrees / 180f * MathF.PI;
      var rotateYaw =
          Quaternion.CreateFromYawPitchRoll(angle, 0, 0);

      var rotationBuffer = rotateYaw * rootBone.FaceTowardsCameraAdjustment;
      GlTransform.MultMatrix(
          SystemMatrix4x4Util.FromRotation(rotationBuffer));
    }

    var animation = this.sceneModel_.Animation;
    var animationPlaybackManager = this.sceneModel_.AnimationPlaybackManager;

    this.hiddenMeshes_.Clear();
    foreach (var mesh in this.meshes_) {
      if (mesh.DefaultDisplayState == MeshDisplayState.HIDDEN) {
        this.hiddenMeshes_.Add(mesh);
      }
    }

    if (animation != null) {
      animationPlaybackManager.Tick();

      var frame = (float) animationPlaybackManager.Frame;
      this.sceneModel_.BoneTransformManager.CalculateMatrices(
          skeleton.Root,
          model.Skin.BoneWeights,
          (animation, frame),
          BoneWeightTransformType.FOR_RENDERING);
      this.sceneModel_.TextureTransformManager.CalculateMatrices(
          model.MaterialManager.Textures,
          (animation, frame));

      foreach (var meshTracks in animation.MeshTracks) {
        if (!meshTracks.DisplayStates.TryGetAtFrame(
                frame,
                out var displayState)) {
          continue;
        }

        if (displayState == MeshDisplayState.HIDDEN) {
          this.hiddenMeshes_.Add(meshTracks.Mesh);
        } else {
          this.hiddenMeshes_.Remove(meshTracks.Mesh);
        }
      }
    } else {
      this.sceneModel_.TextureTransformManager.CalculateMatrices(
          model.MaterialManager.Textures,
          null);
    }

    this.modelRenderer_.Render();

    if (FinConfig.ShowSkeleton || this.isBoneSelected_) {
      this.SkeletonRenderer.Render();
    }

    foreach (var (bone, boneChildren) in this.children_) {
      GlTransform.PushMatrix();

      GlTransform.MultMatrix(
          this.sceneModel_.BoneTransformManager.GetWorldMatrix(bone).Impl);

      foreach (var child in boneChildren) {
        child.Render();
      }

      GlTransform.PopMatrix();
    }

    GlTransform.PopMatrix();
  }
}