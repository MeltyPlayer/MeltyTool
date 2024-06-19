using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;

using fin.ui.rendering.gl;
using fin.util.time;

using OpenTK.Graphics.OpenGL;

using uni.util.windows;

namespace uni.ui.avalonia.common.gl {
  public abstract class BaseTkOpenGlControl
      : OpenGlControlBase, ICustomHitTest {
    private AvaloniaTkContext? avaloniaTkContext_;
    private TimedCallback timedCallback_;

    private static float DEFAULT_FRAMERATE_ { get; } =
      EnumDisplaySettingsUtil.GetDisplayFrequency();

    protected abstract void InitGl();
    protected abstract void RenderGl();
    protected abstract void TeardownGl();


    protected sealed override void OnOpenGlInit(GlInterface gl) {
      //Initialize the OpenTK<->Avalonia Bridge
      this.avaloniaTkContext_ = new AvaloniaTkContext(gl);
      GL.LoadBindings(this.avaloniaTkContext_);

      GlUtil.SwitchContext(null);

      this.InitGl();

      this.timedCallback_ = TimedCallback.WithFrequency(
          () => Dispatcher.UIThread.Post(this.RequestNextFrameRendering,
                                         DispatcherPriority.Background),
          DEFAULT_FRAMERATE_);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
      => this.RenderGl();

    protected sealed override void OnOpenGlDeinit(GlInterface gl)
      => this.TeardownGl();

    public bool HitTest(Point point) => this.Bounds.Contains(point);
  }
}