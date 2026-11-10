using Avalonia.Threading;

namespace uni.ui.avalonia.helpers;

internal static class WaitExtensions {
  public static async Task UntilInputIsProcessed()
    => await Dispatcher
             .UIThread
             .InvokeAsync(() => { }, DispatcherPriority.Input);
}