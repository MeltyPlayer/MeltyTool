using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace uni.ui.avalonia.util;

public static class UtilExtensions {
  public static void ShowNewWindow(this Visual component,
                                   Func<Window> windowCreator)
    => Dispatcher.UIThread.Invoke(() => {
      var window = windowCreator();
      if (TopLevel.GetTopLevel(component) is Window parentWindow) {
        window.Show(parentWindow);
      } else {
        window.Show();
      }
    });

  public static void ShowNewWindow(Func<Window> windowCreator)
    => Dispatcher.UIThread.Invoke(() => { windowCreator().Show(); });
}