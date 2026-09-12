using Avalonia.Input;

namespace uni.ui.avalonia.helpers;

internal static class InputElementExtensions {
  extension(InputElement inputElement) {
    public void Click() => MouseTestHelper.Click(inputElement);
  }
}