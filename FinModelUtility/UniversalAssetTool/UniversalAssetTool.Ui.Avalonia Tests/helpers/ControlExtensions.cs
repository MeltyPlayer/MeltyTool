using Avalonia.Controls;

using fin.ui;
using fin.ui.avalonia.controls;

namespace uni.ui.avalonia.helpers;

internal static class ControlExtensions {
  extension<TControl, TViewModel>(TControl)
      where TControl : BUserControl<TViewModel>, new()
      where TViewModel : IViewModelBase {
    public static TControl Bootstrap(TViewModel viewModel) {
      var control = new TControl { ViewModel = viewModel };

      var window = new Window { Content = control };
      window.Show();

      return control;
    }
  }

  extension(Control control) {
    public string? Text => control switch {
        Button button                           => button.Text,
        SelectableTextBlock selectableTextBlock => selectableTextBlock.Text,
        TextBlock textBlock                     => textBlock.Text,
        TextBox textBox                         => textBox.Text,
        _                                       => null,
    };
  }
}