using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;

using fin.ui.rendering.gl;

using OpenTK.Graphics.OpenGL;

namespace uni.ui.avalonia.common.gl {
  public abstract class BaseTkOpenGlControl
      : OpenGlControlBase, ICustomHitTest {
    private AvaloniaTkContext? avaloniaTkContext_;

    protected abstract void InitGl();
    protected abstract void RenderGl();
    protected abstract void TeardownGl();


    protected sealed override void OnOpenGlInit(GlInterface gl) {
      //Initialize the OpenTK<->Avalonia Bridge
      this.avaloniaTkContext_ = new AvaloniaTkContext(gl);
      GL.LoadBindings(this.avaloniaTkContext_);

      GlUtil.SwitchContext(null);

      this.InitGl();
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb) {
      Dispatcher.UIThread.Post(this.RequestNextFrameRendering,
                               DispatcherPriority.Render);
      this.RenderGl();
    }

    protected sealed override void OnOpenGlDeinit(GlInterface gl)
      => this.TeardownGl();

    public bool HitTest(Point point) => this.Bounds.Contains(point);
  }
}