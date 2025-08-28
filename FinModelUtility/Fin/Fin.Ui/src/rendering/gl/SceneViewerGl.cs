using fin.animation;
using fin.io.web;
using fin.model;
using fin.scene;
using fin.services;
using fin.ui.rendering.gl.model;
using fin.ui.rendering.gl.scene;
using fin.ui.rendering.viewer;
using fin.util.time;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public sealed class SceneViewerGl : ISceneViewer, IRenderable {
  private InfiniteGridRenderer infiniteGridRenderer_ = new();
  private SkyboxRenderer skyboxRenderer_ = new();
  private BackgroundRenderer backgroundRenderer_ = new();

  private ISceneInstance? scene_;
  private SceneRenderer? sceneRenderer_;

  private ISceneAreaInstance? singleArea_;
  private SceneAreaRenderer? singleAreaRenderer_;

  public ISceneInstance? Scene {
    get => this.scene_;
    set {
      this.sceneRenderer_?.Dispose();

      if (value == null) {
        this.scene_ = null;
        this.sceneRenderer_ = null;
        this.singleArea_ = null;
        this.singleAreaRenderer_ = null;
        this.backgroundRenderer_.BackgroundImage = null;
        this.ViewerScale = 1;
      } else {
        this.scene_ = value;

        this.sceneRenderer_ = new SceneRenderer(this.scene_);

        var areas = this.scene_?.Areas;
        this.singleArea_ = areas is { Count: 1 } ? areas[0] : null;

        var areaRenderers = this.sceneRenderer_.AreaRenderers;
        this.singleAreaRenderer_ = areaRenderers is { Count: 1 }
            ? areaRenderers[0]
            : null;

        var singleAreaDefinition = this.singleArea_?.Definition;
        this.backgroundRenderer_.BackgroundImage
            = singleAreaDefinition?.BackgroundImage;
        this.backgroundRenderer_.BackgroundImageScale
            = singleAreaDefinition?.BackgroundImageScale ?? 1;
      }
    }
  }

  public ISceneModelInstance? FirstSceneModel
    => this.Scene
           ?.Areas.FirstOrDefault()
           ?.Objects.FirstOrDefault()
           ?.Models.FirstOrDefault();

  public IAnimationPlaybackManager? AnimationPlaybackManager
    => this.FirstSceneModel?.AnimationPlaybackManager;

  public ISkeletonRenderer? SkeletonRenderer
    => this.sceneRenderer_
           ?.AreaRenderers.FirstOrDefault()
           ?.ObjectRenderers.FirstOrDefault()
           ?.ModelRenderers.FirstOrDefault()
           ?.SkeletonRenderer;

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

  public float ViewerScale {
    get;
    set {
      field = value;
      if (this.scene_ != null) {
        this.scene_.ViewerScale = value;
      }
    }
  } = 1;

  public float GlobalScale { get; set; } = 1;
  public float NearPlane { get; set; }
  public float FarPlane { get; set; }
  public bool ShowGrid { get; set; }

  public void Render() {
    FrameTime.MarkStartOfFrame();
    this.singleArea_?.CustomSkyboxObject?.Tick();
    this.Scene?.Tick();

    var width = this.Width;
    var height = this.Height;
    GL.Viewport(0, 0, width, height);

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

      this.skyboxRenderer_.NearPlane
          = this.infiniteGridRenderer_.NearPlane = glNearPlane;
      this.skyboxRenderer_.FarPlane
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

      var customSkyboxRenderer
          = this.singleAreaRenderer_?.CustomSkyboxRenderer;

      if (this.backgroundRenderer_.IsValid) {
        GlTransform.Ortho2d(0, width, height, 0);
        GlTransform.Translate(hWidth, hHeight, 0);
        GlTransform.Scale(hWidth, hHeight, 1);

        this.backgroundRenderer_.Render();
      } else if (customSkyboxRenderer != null) {
        GlTransform.Translate(this.Camera.Position);
        GlTransform.Scale(this.GlobalScale, this.GlobalScale, this.GlobalScale);
        customSkyboxRenderer.Render();
      } else {
        GlTransform.Ortho2d(0, width, height, 0);
        GlTransform.Translate(hWidth, hHeight, 0);
        GlTransform.Scale(hWidth, hHeight, 1);

        this.skyboxRenderer_.Render();
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
    GlTransform.Scale(this.GlobalScale, this.GlobalScale, this.GlobalScale);
    GlTransform.Rotate(90, 1, 0, 0);
    GlTransform.Scale(this.ViewerScale,
                      this.ViewerScale,
                      this.ViewerScale);

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