using System;
using System.Linq;
using System.Windows.Forms;

using fin.animation;
using fin.importers;
using fin.io.bundles;
using fin.model;
using fin.scene;
using fin.ui;
using fin.ui.rendering;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;
using fin.ui.rendering.gl.scene;

using OpenTK.Graphics.OpenGL;

using uni.config;
using uni.model;
using uni.ui.common;

namespace uni.ui.winforms.common.scene {
  public class SceneViewerGlPanel : BGlPanel, ISceneViewerPanel {
    private readonly Camera camera_ =
        Camera.NewLookingAt(0, 0, 0, 45, -10, 1.5f);

    private float fovY_ = 30;

    private BackgroundSphereRenderer backgroundRenderer_ = new();
    private GridRenderer gridRenderer_ = new();

    private float viewerScale_ = 1;

    private IScene? scene_;
    private SceneRenderer? sceneRenderer_;

    private ISceneArea? singleArea_;
    private SceneAreaRenderer? singleAreaRenderer_;

    private I3dFileBundle? fileBundle_;

    public TimeSpan FrameTime { get; private set; }

    public (I3dFileBundle, IScene)? FileBundleAndScene {
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
          this.viewerScale_ = 1;
        } else {
          (this.fileBundle_, this.scene_) = value.Value;

          this.sceneRenderer_ = new SceneRenderer(this.scene_);

          var areas = this.scene_?.Areas;
          this.singleArea_ = areas is { Count: 1 } ? areas[0] : null;

          var areaRenderers = this.sceneRenderer_.AreaRenderers;
          this.singleAreaRenderer_ = areaRenderers is { Count: 1 }
              ? areaRenderers[0]
              : null;

          this.viewerScale_ = this.scene_.ViewerScale =
              new ScaleSource(Config.Instance.ViewerSettings
                                    .ViewerModelScaleSource).GetScale(
                  this.scene_,
                  this.fileBundle_);
        }
      }
    }

    private IScene? Scene => this.scene_;

    public ISceneModel? FirstSceneModel
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

    private bool isMouseDown_ = false;
    private (int, int)? prevMousePosition_ = null;

    private bool isForwardDown_ = false;
    private bool isBackwardDown_ = false;
    private bool isLeftwardDown_ = false;
    private bool isRightwardDown_ = false;
    private bool isUpwardDown_ = false;
    private bool isDownwardDown_ = false;
    private bool isSpeedupActive_ = false;
    private bool isSlowdownActive_ = false;

    public SceneViewerGlPanel() {
      this.impl_.MouseDown += (_, args) => {
        if (args.Button == MouseButtons.Left ||
            args.Button == MouseButtons.Right) {
          isMouseDown_ = true;
          this.prevMousePosition_ = null;
        }
      };
      this.impl_.MouseUp += (_, args) => {
        if (args.Button == MouseButtons.Left ||
            args.Button == MouseButtons.Right) {
          isMouseDown_ = false;
        }
      };
      this.impl_.MouseMove += (_, args) => {
        if (this.isMouseDown_) {
          var mouseLocation = (args.X, args.Y);

          if (this.prevMousePosition_ != null) {
            var (prevMouseX, prevMouseY) = this.prevMousePosition_.Value;
            var (mouseX, mouseY) = mouseLocation;

            var deltaMouseX = mouseX - prevMouseX;
            var deltaMouseY = mouseY - prevMouseY;

            var fovY = this.fovY_;
            var fovX = fovY / this.Height * this.Width;

            var deltaXFrac = 1f * deltaMouseX / this.Width;
            var deltaYFrac = 1f * deltaMouseY / this.Height;

            var mouseSpeed = 3;

            this.camera_.PitchDegrees = float.Clamp(
                this.camera_.PitchDegrees - deltaYFrac * fovY * mouseSpeed,
                -90,
                90);
            this.camera_.YawDegrees -= deltaXFrac * fovX * mouseSpeed;
          }

          this.prevMousePosition_ = mouseLocation;
        }
      };

      this.impl_.KeyDown += (_, args) => {
        switch (args.KeyCode) {
          case Keys.W: {
            this.isForwardDown_ = true;
            break;
          }
          case Keys.S: {
            this.isBackwardDown_ = true;
            break;
          }
          case Keys.A: {
            this.isLeftwardDown_ = true;
            break;
          }
          case Keys.D: {
            this.isRightwardDown_ = true;
            break;
          }
          case Keys.Q: {
            this.isDownwardDown_ = true;
            break;
          }
          case Keys.E: {
            this.isUpwardDown_ = true;
            break;
          }
          case Keys.ShiftKey: {
            this.isSpeedupActive_ = true;
            break;
          }
          case Keys.ControlKey: {
            this.isSlowdownActive_ = true;
            break;
          }
        }
      };

      this.impl_.KeyUp += (_, args) => {
        switch (args.KeyCode) {
          case Keys.W: {
            this.isForwardDown_ = false;
            break;
          }
          case Keys.S: {
            this.isBackwardDown_ = false;
            break;
          }
          case Keys.A: {
            this.isLeftwardDown_ = false;
            break;
          }
          case Keys.D: {
            this.isRightwardDown_ = false;
            break;
          }
          case Keys.Q: {
            this.isDownwardDown_ = false;
            break;
          }
          case Keys.E: {
            this.isUpwardDown_ = false;
            break;
          }
          case Keys.ShiftKey: {
            this.isSpeedupActive_ = false;
            break;
          }
          case Keys.ControlKey: {
            this.isSlowdownActive_ = false;
            break;
          }
        }
      };
    }

    protected override void InitGl() => this.ResetGl_();
    private void ResetGl_() => GlUtil.ResetGl();

    protected override void RenderGl() {
      var start = DateTime.Now;

      fin.util.time.FrameTime.MarkStartOfFrame();
      this.singleArea_?.CustomSkyboxObject?.Tick();
      this.Scene?.Tick();

      var forwardVector =
          (this.isForwardDown_ ? 1 : 0) - (this.isBackwardDown_ ? 1 : 0);
      var rightwardVector =
          (this.isRightwardDown_ ? 1 : 0) - (this.isLeftwardDown_ ? 1 : 0);
      var upwardVector =
          (this.isUpwardDown_ ? 1 : 0) - (this.isDownwardDown_ ? 1 : 0);

      var cameraSpeed = DebugFlags.GLOBAL_SCALE * 15;
      if (this.isSpeedupActive_) {
        cameraSpeed *= 2;
      }

      if (this.isSlowdownActive_) {
        cameraSpeed /= 2;
      }

      this.camera_.Move(forwardVector,
                        rightwardVector,
                        upwardVector,
                        cameraSpeed);

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
        GlTransform.Perspective(this.fovY_,
                                1.0 * width / height,
                                DebugFlags.NEAR_PLANE,
                                DebugFlags.FAR_PLANE);
        GlTransform.LookAt(this.camera_.X,
                           this.camera_.Y,
                           this.camera_.Z,
                           this.camera_.X + this.camera_.XNormal,
                           this.camera_.Y + this.camera_.YNormal,
                           this.camera_.Z + this.camera_.ZNormal,
                           this.camera_.XUp,
                           this.camera_.YUp,
                           this.camera_.ZUp);
      }

      {
        GlTransform.MatrixMode(TransformMatrixMode.VIEW);
        GlTransform.LoadIdentity();
        GlTransform.Translate(this.camera_.X,
                              this.camera_.Y,
                              this.camera_.Z * .995f);

        GlTransform.MatrixMode(TransformMatrixMode.MODEL);
        GlTransform.Scale(DebugFlags.GLOBAL_SCALE,
                          DebugFlags.GLOBAL_SCALE,
                          DebugFlags.GLOBAL_SCALE);

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
        GlTransform.Scale(DebugFlags.GLOBAL_SCALE,
                          DebugFlags.GLOBAL_SCALE,
                          DebugFlags.GLOBAL_SCALE);

        if (Config.Instance.ViewerSettings.ShowGrid) {
          CommonShaderPrograms.TEXTURELESS_SHADER_PROGRAM.Use();
          this.gridRenderer_.Render();
        }

        {
          GlTransform.Rotate(90, 1, 0, 0);
          GlTransform.Scale(this.viewerScale_,
                            this.viewerScale_,
                            this.viewerScale_);
        }

        this.sceneRenderer_?.Render();
      }
    }
  }
}