using System.Windows.Forms;

using fin.ui.rendering.gl;
using fin.util.time;

using OpenTK.Graphics.OpenGL;

using uni.util.windows;

namespace uni.ui.winforms.common {
  public abstract partial class BGlPanel : UserControl {
    private readonly TimedCallback timedCallback;

    private static float DEFAULT_FRAMERATE_ { get; } = 
      EnumDisplaySettingsUtil.GetDisplayFrequency();

    protected BGlPanel() {
      InitializeComponent();

      if (!DesignModeUtil.InDesignMode) {
        GlUtil.Init();
        this.impl_.CreateGraphics();
        this.impl_.MakeCurrent();

        GlUtil.SwitchContext(this.impl_.Context);
        this.InitGl();

        this.timedCallback =
            TimedCallback.WithFrequency(this.Invalidate, DEFAULT_FRAMERATE_);
      }
    }

    public float Framerate {
      get => this.timedCallback.Frequency;
      set => this.timedCallback.Frequency = value;
    }

    protected abstract void InitGl();

    protected abstract void RenderGl();

    protected override void OnPaint(PaintEventArgs pe) {
      base.OnPaint(pe);
      if (!DesignModeUtil.InDesignMode) {
        // TODO: This may not actually be needed? The concern is whether or not
        // makeCurrent is potentially a race condition
        GlUtil.RunLockedGl(() => {
          GlUtil.SwitchContext(this.impl_.Context);
          this.impl_.MakeCurrent();

          this.RenderGl();

          GL.Flush();
          this.impl_.SwapBuffers();

          this.impl_.Context.MakeNoneCurrent();
        });
      }
    }
  }
}