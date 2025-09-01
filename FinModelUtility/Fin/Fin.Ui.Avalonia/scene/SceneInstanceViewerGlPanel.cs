using Avalonia.Input;
using Avalonia.Interactivity;

using fin.animation;
using fin.model;
using fin.scene;
using fin.services;
using fin.ui.avalonia.gl;
using fin.ui.rendering;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;

namespace fin.ui.avalonia.scene;

public class SceneInstanceViewerGlPanel : BOpenTkControl, ISceneViewer {
  private readonly SceneViewerGl viewerImpl_ = new();

  private bool isMouseDown_ = false;
  private (float, float)? prevMousePosition_ = null;

  private bool isForwardDown_ = false;
  private bool isBackwardDown_ = false;
  private bool isLeftwardDown_ = false;
  private bool isRightwardDown_ = false;
  private bool isUpwardDown_ = false;
  private bool isDownwardDown_ = false;
  private bool isSpeedupActive_ = false;
  private bool isSlowdownActive_ = false;

  public SceneInstanceViewerGlPanel() {
    this.AddHandler(
        PointerPressedEvent,
        (_, args) => {
          var currentPoint = args.GetCurrentPoint(this);
          var properties = currentPoint.Properties;

          if (properties.IsLeftButtonPressed ||
              properties.IsRightButtonPressed) {
            this.isMouseDown_ = true;
            this.prevMousePosition_ = null;
          }
        },
        RoutingStrategies.Tunnel);
    this.AddHandler(
        PointerReleasedEvent,
        (_, args) => {
          var currentPoint = args.GetCurrentPoint(this);
          var properties = currentPoint.Properties;

          if (properties.PointerUpdateKind is
              PointerUpdateKind.LeftButtonReleased
              or PointerUpdateKind.RightButtonReleased) {
            this.isMouseDown_ = false;
          }
        },
        RoutingStrategies.Tunnel);

    this.AddHandler(
        PointerMovedEvent,
        (_, args) => {
          var currentPoint = args.GetCurrentPoint(this);
          var position = currentPoint.Position;

          if (this.isMouseDown_) {
            var mouseLocation = ((float) position.X, (float) position.Y);

            if (this.prevMousePosition_ != null) {
              var width = (int) this.Bounds.Width;
              var height = (int) this.Bounds.Height;

              var (prevMouseX, prevMouseY)
                  = this.prevMousePosition_.Value;
              var (mouseX, mouseY) = mouseLocation;

              var deltaMouseX = mouseX - prevMouseX;
              var deltaMouseY = mouseY - prevMouseY;

              var fovY = this.viewerImpl_.FovY;
              var fovX = (fovY / height * width);

              var deltaXFrac = 1f * deltaMouseX / width;
              var deltaYFrac = 1f * deltaMouseY / height;

              var mouseSpeed = 3;

              this.Camera.PitchDegrees = float.Clamp(
                  this.Camera.PitchDegrees -
                  deltaYFrac * fovY * mouseSpeed,
                  -90,
                  90);
              this.Camera.YawDegrees -= deltaXFrac * fovX * mouseSpeed;
            }

            this.prevMousePosition_ = mouseLocation;
          }
        },
        RoutingStrategies.Tunnel);

    this.AddHandler(
        KeyDownEvent,
        (_, args) => {
          switch (args.Key) {
            case Key.W: {
              this.isForwardDown_ = true;
              break;
            }
            case Key.S: {
              this.isBackwardDown_ = true;
              break;
            }
            case Key.A: {
              this.isLeftwardDown_ = true;
              break;
            }
            case Key.D: {
              this.isRightwardDown_ = true;
              break;
            }
            case Key.Q: {
              this.isDownwardDown_ = true;
              break;
            }
            case Key.E: {
              this.isUpwardDown_ = true;
              break;
            }
            case Key.LeftShift:
            case Key.RightShift: {
              this.isSpeedupActive_ = true;
              break;
            }
            case Key.LeftCtrl:
            case Key.RightCtrl: {
              this.isSlowdownActive_ = true;
              break;
            }
          }
        },
        handledEventsToo: true);

    this.AddHandler(
        KeyUpEvent,
        (_, args) => {
          switch (args.Key) {
            case Key.W: {
              this.isForwardDown_ = false;
              break;
            }
            case Key.S: {
              this.isBackwardDown_ = false;
              break;
            }
            case Key.A: {
              this.isLeftwardDown_ = false;
              break;
            }
            case Key.D: {
              this.isRightwardDown_ = false;
              break;
            }
            case Key.Q: {
              this.isDownwardDown_ = false;
              break;
            }
            case Key.E: {
              this.isUpwardDown_ = false;
              break;
            }
            case Key.LeftShift:
            case Key.RightShift: {
              this.isSpeedupActive_ = false;
              break;
            }
            case Key.LeftCtrl:
            case Key.RightCtrl: {
              this.isSlowdownActive_ = false;
              break;
            }
          }
        },
        handledEventsToo: true);
  }

  protected override void InitGl() => GlUtil.ResetGl();
  protected override void TeardownGl() { }

  protected override void RenderGl() {
    if (LoadingStatusService.IsLoading) {
      return;
    }

    if (this.AllowMovingCamera) {
      var forwardVector =
          (this.isForwardDown_ ? 1 : 0) - (this.isBackwardDown_ ? 1 : 0);
      var rightwardVector =
          (this.isRightwardDown_ ? 1 : 0) - (this.isLeftwardDown_ ? 1 : 0);
      var upwardVector =
          (this.isUpwardDown_ ? 1 : 0) - (this.isDownwardDown_ ? 1 : 0);

      var cameraSpeed = UiConstants.GLOBAL_SCALE * 15;
      if (this.isSpeedupActive_) {
        cameraSpeed *= 2;
      }

      if (this.isSlowdownActive_) {
        cameraSpeed /= 2;
      }

      this.Camera.Move(forwardVector,
                       rightwardVector,
                       upwardVector,
                       cameraSpeed);
    }

    this.GetBoundsForGlViewport(out var width, out var height);
    this.viewerImpl_.Width = width;
    this.viewerImpl_.Height = height;

    this.viewerImpl_.GlobalScale = UiConstants.GLOBAL_SCALE;
    this.viewerImpl_.NearPlane = UiConstants.NEAR_PLANE;
    this.viewerImpl_.FarPlane = UiConstants.FAR_PLANE;

    this.viewerImpl_.Render();
  }

  public ISceneInstance? Scene {
    get => this.viewerImpl_.Scene;
    set => this.viewerImpl_.Scene = value;
  }

  public ISceneModelInstance? FirstSceneModel
    => this.viewerImpl_.FirstSceneModel;

  public IAnimationPlaybackManager? AnimationPlaybackManager
    => this.viewerImpl_.AnimationPlaybackManager;

  public IReadOnlyModelAnimation? Animation {
    get => this.viewerImpl_.Animation;
    set => this.viewerImpl_.Animation = value;
  }

  public ISkeletonRenderer? SkeletonRenderer
    => this.viewerImpl_.SkeletonRenderer;

  public Camera Camera => this.viewerImpl_.Camera;

  public bool ShowGrid {
    get => this.viewerImpl_.ShowGrid;
    set => this.viewerImpl_.ShowGrid = value;
  }

  public float ViewerScale {
    get => this.viewerImpl_.ViewerScale;
    set => this.viewerImpl_.ViewerScale = value;
  }

  public bool AllowMovingCamera { get; set; } = true;

  public bool UseOrthoCamera {
    get => this.viewerImpl_.UseOrthoCamera;
    set => this.viewerImpl_.UseOrthoCamera = value;
  }
}