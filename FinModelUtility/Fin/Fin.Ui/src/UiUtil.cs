using System.Globalization;
using System.Text;

using fin.image;
using fin.ui.rendering.gl;
using fin.util.time;

namespace fin.ui;

public static class UiUtil {
  public static void Initialize() {
    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    FrameTime.Initialize();
    GpuUtil.Initialize();
    FinImage.Initialize();
  }
}
