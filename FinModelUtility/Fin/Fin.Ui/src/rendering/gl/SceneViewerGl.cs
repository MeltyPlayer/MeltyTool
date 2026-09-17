using System.Drawing;

using fin.animation;
using fin.io.web;
using fin.model;
using fin.scene;
using fin.scene.components;
using fin.services;
using fin.ui.rendering.gl.scene;
using fin.ui.rendering.gl.ubo;
using fin.ui.rendering.viewer;
using fin.util.time;

namespace fin.ui.rendering.gl;

public sealed class SceneViewerGl : ISceneViewer, IRenderable {
  private InfiniteGridRenderer infiniteGridRenderer_ = new();
  private BackgroundRenderer backgroundRenderer_ = new();

  private ISceneInstance? scene_;
  private SceneRenderer? sceneRenderer_;

  private ISceneAreaInstance? singleArea_;
  private SceneNodeRenderer? customSkyboxRenderer_;

  private ViewMatricesUbo? viewMatricesUbo_;

  ~SceneViewerGl() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.infiniteGridRenderer_.Dispose();
    this.backgroundRenderer_.Dispose();
    this.BackdropRenderer?.Dispose();
    this.scene_?.Dispose();
    this.sceneRenderer_?.Dispose();
  }

  public bool UseRenderGraph { get; set; } = true;

  public ISceneInstance? Scene {
    get => this.scene_;
    set {
      this.sceneRenderer_?.Dispose();

      if (value == null) {
        this.scene_ = null;
        this.sceneRenderer_ = null;
        this.singleArea_ = null;
        this.customSkyboxRenderer_ = null;
        this.backgroundRenderer_.BackgroundImage = null;
        this.ViewerScale = 1;
      } else {
        this.scene_ = value;

        this.sceneRenderer_
            = new SceneRenderer(this.scene_, this.UseRenderGraph);

        var areas = this.scene_?.Areas;
        this.singleArea_ = areas is { Count: 1 } ? areas[0] : null;

        this.customSkyboxRenderer_
            = this.singleArea_?.CustomSkyboxObject is { } customSkyboxObject
                ? new SceneNodeRenderer(customSkyboxObject)
            : null;

        var singleAreaDefinition = this.singleArea_?.Definition;
        this.backgroundRenderer_.BackgroundImage
            = singleAreaDefinition?.BackgroundImage;
        this.backgroundRenderer_.BackgroundImageScale
            = singleAreaDefinition?.BackgroundImageScale ?? 1;
      }
    }
  }

  public IAnimatableModel? FirstSceneModel
    => this.Scene?.EnumerateAllNonSkyboxAnimatableModels().FirstOrDefault();

  public IAnimationPlaybackManager? AnimationPlaybackManager
    => this.FirstSceneModel?.AnimationPlaybackManager;

  public IReadOnlyModelAnimation? Animation {
    get => this.FirstSceneModel?.Animation;
    set {
      if (this.FirstSceneModel == null) {
        return;
      }

      this.FirstSceneModel.Animation = value;
    }
  }

  public Camera Camera { get; } =
    Camera.NewLookingAt(0, 0, 0, 45, -10, 1.5f);

  public bool UseOrthoCamera { get; set; } = false;

  public float FovY => 30;

  public int Width { get; set; }
  public int Height { get; set; }

  public float ViewerScale { get; set; } = 1;
  public float GlobalScale { get; set; } = 1;
  
  public float NearPlane { get; set; }
  public float FarPlane { get; set; }
  
  public bool ShowGrid { get; set; }
  public IOrthoRenderable? BackdropRenderer { get; set; } = new SkyboxRenderer();

  public void Render() {
    FrameTime.MarkStartOfFrame();
    this.singleArea_?.CustomSkyboxObject?.Tick();
    this.Scene?.Tick();

    var width = this.Width;
    var height = this.Height;
    GlUtil.SetViewport(new Rectangle(0, 0, width, height));

    // Dynamically update the near/far planes when far away to prevent clipping.
    var glNearPlane = this.NearPlane;
    var glFarPlane = this.FarPlane;
    {
      var distanceFromOrigin = this.Camera.Position.Length();
      var maxDistance = 10;
      if (distanceFromOrigin > maxDistance) {
        glNearPlane += distanceFromOrigin - maxDistance;
        glFarPlane += distanceFromOrigin - maxDistance;
      }

      this.BackdropRenderer?.ViewportWidth = width;
      this.BackdropRenderer?.ViewportHeight = height;

      this.sceneRenderer_?.Scale = this.GlobalScale * this.ViewerScale;
      this.sceneRenderer_?.NearPlane = glNearPlane;
      this.sceneRenderer_?.FarPlane = glFarPlane;

      var skyboxRenderer = this.BackdropRenderer as ISkyboxRenderer;
      skyboxRenderer?.NearPlane
          = this.infiniteGridRenderer_.NearPlane = glNearPlane;
      skyboxRenderer?.FarPlane
          = this.infiniteGridRenderer_.FarPlane = glFarPlane;
    }

    var singleAreaDefinition = this.singleArea_?.Definition;
    if (singleAreaDefinition?.BackgroundColor != null) {
      GlUtil.SetClearColor(singleAreaDefinition.BackgroundColor.Value);
    }

    GlUtil.ClearColorAndDepth();

    {
      GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
      GlTransform.LoadIdentity();

      if (!this.UseOrthoCamera) {
        GlTransform.Perspective(this.FovY,
                                1.0 * width / height,
                                glNearPlane,
                                glFarPlane);
      } else {
        GlTransform.Ortho2d(-1,
                            1,
                            -1,
                            1,
                            glNearPlane,
                            glFarPlane);
      }
    }

    {
      GlTransform.MatrixMode(TransformMatrixMode.VIEW);
      GlTransform.LoadIdentity();
      GlTransform.LookAt(this.Camera.Position,
                         this.Camera.Position + this.Camera.Normal,
                         this.Camera.Up);
    }

    if (this.viewMatricesUbo_ == null) {
      this.viewMatricesUbo_ = new();
    }
    this.viewMatricesUbo_.UpdateData();
    this.viewMatricesUbo_.Bind();

    this.RenderSkybox_();
    this.RenderScene_();

    FrameTime.MarkEndOfFrameForFpsDisplay();
  }

  private void RenderSkybox_() {
    var width = this.Width;
    var height = this.Height;

    var hWidth = width / 2f;
    var hHeight = height / 2f;

    {
      GlTransform.MatrixMode(TransformMatrixMode.MODEL);
      GlTransform.LoadIdentity();

      if (this.backgroundRenderer_.IsValid) {
        GlTransform.Ortho2d(0, width, height, 0);
        GlTransform.Translate(hWidth, hHeight, 0);
        GlTransform.Scale(hWidth, hHeight, 1);

        this.backgroundRenderer_.AspectRatio = hWidth / hHeight;
        this.backgroundRenderer_.Render();
      } else if (this.customSkyboxRenderer_ != null) {
        GlTransform.Translate(this.Camera.Position);
        GlTransform.Scale(this.GlobalScale);
        this.customSkyboxRenderer_.Render();
      } else {
        this.BackdropRenderer?.Render();
      }
    }

    if (this.ShowGrid) {
      GlTransform.LoadIdentity();
      GlTransform.Ortho2d(0, width, height, 0);
      GlTransform.Translate(hWidth, hHeight, 0);
      GlTransform.Scale(hWidth, hHeight, 1);

      this.infiniteGridRenderer_.Render();
    }
  }

  private void RenderScene_() {
    GlTransform.MatrixMode(TransformMatrixMode.MODEL);
    GlTransform.LoadIdentity();
    GlTransform.Scale(this.GlobalScale * this.ViewerScale);
    GlTransform.Rotate(90, 1, 0, 0);

    try {
      this.sceneRenderer_?.Render();
    } catch (Exception e) {
      ExceptionService.HandleException(e,
                                       new RenderFileBundleExceptionContext(
                                           this.Scene!.Definition.FileBundle));
      this.sceneRenderer_ = null;
    }
  }
}