using fin.animation;
using fin.importers;
using fin.model;
using fin.scene;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;
using fin.ui.rendering.gl.scene;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl {
  public class SceneViewerGl : ISceneViewer, IRenderable {
    private float viewerScale_ = 1;

    private BackgroundSphereRenderer backgroundRenderer_ = new();
    private GridRenderer gridRenderer_ = new();

    private ISceneInstance? scene_;
    private SceneRenderer? sceneRenderer_;

    private ISceneAreaInstance? singleArea_;
    private SceneAreaRenderer? singleAreaRenderer_;

    private I3dFileBundle? fileBundle_;

    public TimeSpan FrameTime { get; private set; }

    public (I3dFileBundle, ISceneInstance)? FileBundleAndScene {
      get {
        var scene = this.scene_;
        return scene != null
            ? (this.fileBundle_!, scene)
            : null;
      }
      set {
        this.sceneRenderer_?.Dispose();

        if (value == null) {
          this.fileBundle_ = null;
          this.scene_ = null;
          this.sceneRenderer_ = null;
          this.singleArea_ = null;
          this.singleAreaRenderer_ = null;
          this.ViewerScale = 1;
        } else {
          (this.fileBundle_, this.scene_) = value.Value;

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

    private ISceneInstance? Scene => this.scene_;

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

    public void Render() {
      var start = DateTime.Now;

      fin.util.time.FrameTime.MarkStartOfFrame();
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

      var end = DateTime.Now;
      this.FrameTime = end - start;
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

      {
        GlTransform.MatrixMode(TransformMatrixMode.VIEW);
        GlTransform.LoadIdentity();
        GlTransform.Translate(this.Camera.X,
                              this.Camera.Y,
                              this.Camera.Z * .995f);

        GlTransform.MatrixMode(TransformMatrixMode.MODEL);
        GlTransform.Scale(this.GlobalScale, this.GlobalScale, this.GlobalScale);

        var skyboxRenderer =
            (IRenderable?) this.singleAreaRenderer_?.CustomSkyboxRenderer ??
            this.backgroundRenderer_;
        skyboxRenderer.Render();
      }

      {
        GlTransform.MatrixMode(TransformMatrixMode.VIEW);
        GlTransform.LoadIdentity();

        GlTransform.MatrixMode(TransformMatrixMode.MODEL);
        GlTransform.LoadIdentity();
        GlTransform.Scale(this.GlobalScale, this.GlobalScale, this.GlobalScale);

        if (this.ShowGrid) {
          CommonShaderPrograms.TEXTURELESS_SHADER_PROGRAM.Use();
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
}