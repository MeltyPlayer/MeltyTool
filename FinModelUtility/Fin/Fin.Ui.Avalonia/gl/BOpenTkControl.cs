using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;

using fin.ui.rendering.gl;
using fin.util.time;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.avalonia.gl;

public abstract class BOpenTkControl
    : OpenGlControlBase, ICustomHitTest {
  private AvaloniaOpenTkContext? avaloniaTkContext_;
  private TimedCallback renderCallback_;

  protected abstract void InitGl();
  protected abstract void RenderGl();
  protected abstract void TeardownGl();

  private static bool isLoaded_ = false;

  protected sealed override void OnOpenGlInit(GlInterface gl) {
    if (!isLoaded_) {
      //Initialize the OpenTK<->Avalonia Bridge
      this.avaloniaTkContext_ = new AvaloniaOpenTkContext(gl);

      GL.LoadBindings(this.avaloniaTkContext_);
      isLoaded_ = true;
    }

    GlUtil.SwitchContext(this);
    this.InitGl();

    this.renderCallback_ = TimedCallback.WithFrequency(
        () => Dispatcher.UIThread.Post(this.RequestNextFrameRendering,
                                       DispatcherPriority.Background),
        UiConstants.FPS);
  }

  protected override void OnOpenGlRender(GlInterface gl, int fb) {
    this.RequestNextFrameRendering();

    GlUtil.SwitchContext(this);
    this.RenderGl();
  }

  protected sealed override void OnOpenGlDeinit(GlInterface gl)
    => this.TeardownGl();

  public bool HitTest(Point point) => this.Bounds.Contains(point);

  protected void GetBoundsForGlViewport(out int width, out int height) {
    var scaling = 1f;
    if (TopLevel.GetTopLevel(this) is Window window) {
      scaling = (float) window.RenderScaling;
    }

    var bounds = this.Bounds;
    width = (int) (scaling * bounds.Width);
    height = (int) (scaling * bounds.Height);
  }
}