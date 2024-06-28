using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;

using fin.ui.rendering.gl;

using OpenTK.Graphics.OpenGL;

namespace uni.ui.avalonia.common.gl {
  public abstract class BOpenTkControl
      : OpenGlControlBase, ICustomHitTest {
    private AvaloniaOpenTkContext? avaloniaTkContext_;

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
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb) {
      Dispatcher.UIThread.Invoke(this.RequestNextFrameRendering,
                                 DispatcherPriority.MaxValue);
      GlUtil.SwitchContext(this);
      this.RenderGl();
    }

    protected sealed override void OnOpenGlDeinit(GlInterface gl)
      => this.TeardownGl();

    public bool HitTest(Point point) => this.Bounds.Contains(point);
  }
}