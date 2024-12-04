using fin.animation;
using fin.model;
using fin.scene;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;
using fin.ui.rendering.gl.scene;
using fin.util.time;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public class SceneViewerGl : ISceneViewer, IRenderable {
  private float viewerScale_ = 1;

  private BackgroundSphereRenderer backgroundRenderer_ = new();
  private GridRenderer gridRenderer_ = new();

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

  public float FovY => 30;

  public int Width { get; set; }
  public int Height { get; set; }

  public float ViewerScale {
    get => this.viewerScale_;
    set {
      this.viewerScale_ = value;
      if (this.scene_ != null) {
        this.scene_.ViewerScale = value;
      }
    }
  }

  public float GlobalScale { get; set; } = 1;
  public float NearPlane { get; set; }
  public float FarPlane { get; set; }
  public bool ShowGrid { get; set; }

  public unsafe void Render() {
    FrameTime.MarkStartOfFrame();
    this.singleArea_?.CustomSkyboxObject?.Tick();
    this.Scene?.Tick();

    var width = this.Width;
    var height = this.Height;
    GL.Viewport(0, 0, width, height);

    if (this.singleArea_?.BackgroundColor != null) {
      GlUtil.SetClearColor(this.singleArea_.BackgroundColor.Value);
    }

    GlUtil.ClearColorAndDepth();

    this.RenderPerspective_();

    FrameTime.MarkEndOfFrameForFpsDisplay();
  }

  private void RenderPerspective_() {
    var width = this.Width;
    var height = this.Height;

    {
      GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
      GlTransform.LoadIdentity();
      GlTransform.Perspective(this.FovY,
                              1.0 * width / height,
                              this.NearPlane,
                              this.FarPlane);
    }

    {
      GlTransform.MatrixMode(TransformMatrixMode.VIEW);
      GlTransform.LoadIdentity();
      GlTransform.LookAt(this.Camera.X,
                         this.Camera.Y,
                         this.Camera.Z,
                         this.Camera.X + this.Camera.XNormal,
                         this.Camera.Y + this.Camera.YNormal,
                         this.Camera.Z + this.Camera.ZNormal,
                         this.Camera.XUp,
                         this.Camera.YUp,
                         this.Camera.ZUp);
    }

    GlTransform.MatrixMode(TransformMatrixMode.MODEL);

    {
      GlTransform.LoadIdentity();

      var customSkyboxRenderer
          = this.singleAreaRenderer_?.CustomSkyboxRenderer;
      if (customSkyboxRenderer == null) {
        GlTransform.Translate(this.Camera.X,
                              this.Camera.Y,
                              this.Camera.Z * .995f);
      } else {
        GlTransform.Translate(this.Camera.X,
                              this.Camera.Y,
                              this.Camera.Z);
      }

      GlTransform.Scale(this.GlobalScale, this.GlobalScale, this.GlobalScale);

      var skyboxRenderer = (IRenderable?) customSkyboxRenderer ??
                           this.backgroundRenderer_;
      skyboxRenderer.Render();
    }

    {
      GlTransform.LoadIdentity();
      GlTransform.Scale(this.GlobalScale, this.GlobalScale, this.GlobalScale);

      if (this.ShowGrid) {
        this.gridRenderer_.Render();
      }

      {
        GlTransform.Rotate(90, 1, 0, 0);
        GlTransform.Scale(this.ViewerScale,
                          this.ViewerScale,
                          this.ViewerScale);
      }

      this.sceneRenderer_?.Render();
    }
  }
}