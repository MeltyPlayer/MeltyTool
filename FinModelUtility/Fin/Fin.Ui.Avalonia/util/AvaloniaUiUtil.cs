using Avalonia.Threading;

using fin.services;

namespace fin.ui.avalonia.util;

public static class AvaloniaUiUtil {
  public static void Initialize() {
    UiUtil.Initialize();

    Dispatcher.UIThread.UnhandledException
        += (_, e) => {
          ExceptionService.HandleException(e.Exception, null);
          e.Handled = true;
        };
  }
}
