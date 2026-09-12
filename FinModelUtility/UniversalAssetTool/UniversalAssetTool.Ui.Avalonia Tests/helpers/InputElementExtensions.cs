using Avalonia.Controls;

namespace uni.ui.avalonia.helpers;

internal static class ControlExtensions {
  extension(Control control) {
    public string? Text => control switch {
        Button button       => button.Text,
        TextBlock textBlock => textBlock.Text,
        TextBox textBox     => textBox.Text,
        _                   => null,
    };
  }
}