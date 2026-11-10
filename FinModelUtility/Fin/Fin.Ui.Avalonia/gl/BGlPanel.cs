using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;

using fin.config;
using fin.ui.rendering;
using fin.ui.rendering.gl;

namespace fin.ui.avalonia.gl;

public abstract class BGlPanel : Panel, ICustomHitTest {
  public event Action? OnInit;
  private Window? window_;

  protected BGlPanel() {
    Dispatcher.UIThread.InvokeAsync(async () => {
      var initGl = () => {
        this.InitGl();
        this.OnInit?.Invoke();

        DebugService.VendorName = GlUtil.GetVendor();
        DebugService.RendererName = GlUtil.GetRenderer();
      };
      var renderGl = this.RenderGl;
      var teardownGl = this.TeardownGl;

      if (FinConfig.PreferGlNativeInterop) {
        OpenGlVersionService.Init(false);
        if (await SharpDxInteropControl.TryToAddTo(
                this,
                initGl,
                renderGl,
                teardownGl)) {
          return;
        }
      }

      OpenGlVersionService.Init(true);
      this.Children.Add(new OpenTkControl(initGl, renderGl, teardownGl));
    });
  }

  protected abstract void InitGl();
  protected abstract void RenderGl();
  protected abstract void TeardownGl();

  public bool HitTest(Point point) => this.Bounds.Contains(point);

  protected void GetBoundsForGlViewport(out int width, out int height) {
    this.window_ ??= TopLevel.GetTopLevel(this) as Window;
    var scaling = (float) (this.window_?.RenderScaling ?? 1);

    var bounds = this.Bounds;
    width = (int) (scaling * bounds.Width);
    height = (int) (scaling * bounds.Height);
  }
}