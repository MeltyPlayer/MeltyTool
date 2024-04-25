using System.Numerics;

using fin.config;
using fin.data.dictionaries;
using fin.math;
using fin.math.matrix.four;
using fin.model;
using fin.scene;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;

namespace fin.ui.rendering.gl.scene {
  public class SceneModelRenderer : IRenderable, IDisposable {
    private readonly ISceneModel sceneModel_;
    private readonly IModelRenderer modelRenderer_;

    private readonly ListDictionary<IBone, SceneModelRenderer>
        children_ = new();

    public SceneModelRenderer(ISceneModel sceneModel,
                              IReadOnlyLighting? lighting) {
      this.sceneModel_ = sceneModel;

      var model = sceneModel.Model;
      this.modelRenderer_ =
          new ModelRendererV2(model,
                              lighting,
                              sceneModel.BoneTransformManager);

      this.modelRenderer_.UseLighting =
          new UseLightingDetector().ShouldUseLightingFor(model);

      this.SkeletonRenderer =
          new SkeletonRenderer(model.Skeleton,
                               this.sceneModel_.BoneTransformManager) {
              Scale = this.sceneModel_.ViewerScale
          };

      foreach (var (bone, boneChildren) in sceneModel.Children.GetPairs()) {
        foreach (var child in boneChildren) {
          this.children_.Add(bone, new SceneModelRenderer(child, lighting));
        }
      }
    }

    ~SceneModelRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      this.modelRenderer_.Dispose();
      foreach (var child in this.children_.Values) {
        child.Dispose();
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
      if (animation != null) {
        animationPlaybackManager.Tick();

        var frame = (float) animationPlaybackManager.Frame;
        this.sceneModel_.BoneTransformManager.CalculateMatrices(
            skeleton.Root,
            model.Skin.BoneWeights,
            (animation, frame),
            BoneWeightTransformType.FOR_RENDERING,
            animationPlaybackManager.Config);

        var hiddenMeshes = this.modelRenderer_.HiddenMeshes;

        hiddenMeshes.Clear();
        var defaultDisplayState = MeshDisplayState.VISIBLE;
        foreach (var (mesh, meshTracks) in animation.MeshTracks) {
          if (!meshTracks.DisplayStates.TryGetInterpolatedFrame(
                  frame,
                  out var displayState)) {
            displayState = defaultDisplayState;
          }

          if (displayState == MeshDisplayState.HIDDEN) {
            hiddenMeshes.Add(mesh);
          }
        }
      }

      this.modelRenderer_.Render();

      if (FinConfig.ShowSkeleton) {
        CommonShaderPrograms.TEXTURELESS_SHADER_PROGRAM.Use();
        this.SkeletonRenderer.Render();
      }

      foreach (var (bone, boneChildren) in this.children_.GetPairs()) {
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
}