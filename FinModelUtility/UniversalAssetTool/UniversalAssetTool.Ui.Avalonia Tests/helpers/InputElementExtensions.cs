using Avalonia.Input;
using Avalonia.Threading;

namespace uni.ui.avalonia.helpers;

internal static class InputElementExtensions {
  extension(InputElement inputElement) {
    public void Click() {
      MouseTestHelper.Click(inputElement);

      // In some cases, we need to wait for things to render after click.
      Dispatcher.UIThread.RunJobs();
    }
  }
}