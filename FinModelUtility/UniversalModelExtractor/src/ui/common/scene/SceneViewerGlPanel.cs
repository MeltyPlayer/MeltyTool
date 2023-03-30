using fin.animation.playback;
using fin.gl;
using fin.gl.material;
using fin.gl.model;
using fin.io.bundles;
using fin.model;
using fin.model.util;
using fin.scene;
using fin.ui;
using OpenTK.Graphics.OpenGL;
using uni.config;
using uni.ui.gl;


namespace uni.ui.common.scene {
  public class SceneViewerGlPanel : BGlPanel, ISceneViewerPanel {
    private readonly Camera camera_ = new();
    private float fovY_ = 30;

    private readonly Color backgroundColor_ = Color.FromArgb(51, 128, 179);

    private BackgroundSphereRenderer backgroundRenderer_ = new();
    private GridRenderer gridRenderer_ = new();

    private float viewerScale_ = 1;

    private IScene? scene_;
    private IFileBundle? fileBundle_;

    public (IFileBundle, IScene)? FileBundleAndScene {
      get {
        var scene = this.scene_;
        return scene != null ? (this.fileBundle_!, scene) : null;
      }
      set {
        this.fileBundle_ = value?.Item1;
        var scene = this.scene_ = value?.Item2;

        if (scene != null) {
          this.viewerScale_ = scene.ViewerScale =
                            1000 / SceneScaleCalculator.CalculateScale(scene);
        } else {
          this.viewerScale_ = 1;
        }
      }
    }

    private IScene? Scene => this.FileBundleAndScene?.Item2;

    public ISceneModel? FirstSceneModel
      => this.Scene?.Areas.FirstOrDefault()
             ?.Objects.FirstOrDefault()
             ?.Models.FirstOrDefault();

    public IAnimationPlaybackManager? AnimationPlaybackManager
      => this.FirstSceneModel?.AnimationPlaybackManager;

    public ISkeletonRenderer? SkeletonRenderer
      => this.FirstSceneModel?.SkeletonRenderer;

    public IAnimation? Animation {
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
    private bool isRaiseDown_ = false;
    private bool isLowerDown_ = false;
    private bool isSpeedupActive_ = false;

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

            this.camera_.Pitch -= deltaYFrac * fovY * mouseSpeed;
            this.camera_.Yaw -= deltaXFrac * fovX * mouseSpeed;
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
              this.isLowerDown_ = true;
              break;
            }
          case Keys.E: {
              this.isRaiseDown_ = true;
              break;
            }
          case Keys.ShiftKey: {
              this.isSpeedupActive_ = true;
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
              this.isLowerDown_ = false;
              break;
            }
          case Keys.E: {
              this.isRaiseDown_ = false;
              break;
            }
          case Keys.ShiftKey: {
              this.isSpeedupActive_ = false;
              break;
            }
        }
      };
    }

    protected override void InitGl() {
      ResetGl_();
    }

    private void ResetGl_() {
      GL.ShadeModel(ShadingModel.Smooth);
      GL.Enable(EnableCap.PointSmooth);
      GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

      GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

      GL.ClearDepth(5.0F);

      GL.DepthFunc(DepthFunction.Lequal);
      GL.Enable(EnableCap.DepthTest);
      GL.DepthMask(true);

      GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

      GL.Enable(EnableCap.Texture2D);
      GL.Enable(EnableCap.Normalize);

      GL.Enable(EnableCap.CullFace);
      GL.CullFace(CullFaceMode.Back);

      GL.Enable(EnableCap.Blend);
      GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

      GL.ClearColor(backgroundColor_.R / 255f, backgroundColor_.G / 255f,
                    backgroundColor_.B / 255f, 1);
    }

    protected override void RenderGl() {
      var forwardVector =
          (this.isForwardDown_ ? 1 : 0) - (this.isBackwardDown_ ? 1 : 0);
      var rightwardVector =
          (this.isRightwardDown_ ? 1 : 0) - (this.isLeftwardDown_ ? 1 : 0);
      var upwardVector =
          (this.isRaiseDown_ ? 1 : 0) - (this.isLowerDown_ ? 1 : 0);
      this.camera_.Move(forwardVector, rightwardVector, upwardVector,
                        DebugFlags.GLOBAL_SCALE *
                        (this.isSpeedupActive_ ? 30 : 15));

      var width = this.Width;
      var height = this.Height;
      GL.Viewport(0, 0, width, height);

      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      this.RenderPerspective_();
    }

    private void RenderPerspective_() {
      var width = this.Width;
      var height = this.Height;

      {
        GlTransform.MatrixMode(MatrixMode.Projection);
        GlTransform.LoadIdentity();
        GlTransform.Perspective(this.fovY_, 1.0 * width / height,
                           DebugFlags.NEAR_PLANE, DebugFlags.FAR_PLANE);
        GlTransform.LookAt(this.camera_.X, this.camera_.Y, this.camera_.Z,
                      this.camera_.X + this.camera_.XNormal,
                      this.camera_.Y + this.camera_.YNormal,
                      this.camera_.Z + this.camera_.ZNormal,
                      0, 0, 1);

        GlTransform.MatrixMode(MatrixMode.Modelview);
        GlTransform.LoadIdentity();
      }

      {
        GlTransform.Translate(this.camera_.X, this.camera_.Y, this.camera_.Z * .995f);
        this.backgroundRenderer_.Render();
        GlTransform.LoadIdentity();
      }

      GlTransform.Scale(DebugFlags.GLOBAL_SCALE,
               DebugFlags.GLOBAL_SCALE,
               DebugFlags.GLOBAL_SCALE);

      if (Config.Instance.ShowGrid) {
        CommonShaderPrograms.TEXTURELESS_SHADER_PROGRAM.Use();
        this.gridRenderer_.Render();
      }

      {
        GlTransform.Rotate(90, 1, 0, 0);
        GlTransform.Scale(this.viewerScale_, this.viewerScale_, this.viewerScale_);
      }

      this.Scene?.Tick();
      this.Scene?.Render();
    }
  }
}