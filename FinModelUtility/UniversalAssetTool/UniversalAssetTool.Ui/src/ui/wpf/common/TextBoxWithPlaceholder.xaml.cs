using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;

namespace uni.src.ui.wpf.common {
  /// <summary>
  /// Interaction logic for WatermarkTextBox.xaml
  /// </summary>
  public partial class TextBoxWithPlaceholder : UserControl {
    private readonly TextBoxWithPlaceholderViewModel viewModel_;

    public TextBoxWithPlaceholder(TextBoxWithPlaceholderViewModel viewModel) {
      InitializeComponent();
      this.viewModel_ = viewModel;
    }

    public string Placeholder {
      get => this.viewModel_.Placeholder;
      set => this.viewModel_.Placeholder = value;
    }

    private void TextChanged_(object sender, TextChangedEventArgs args)
      => this.placeholderLabel_.Visibility
          = this.impl_.Text != ""
              ? Visibility.Hidden
              : Visibility.Visible;
  }

  public class TextBoxWithPlaceholderViewModel : INotifyPropertyChanged {
    private string placeholder_ = "Search...";
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Placeholder {
      get => this.placeholder_;
      set {
        this.placeholder_ = value;
        this.PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nameof(Placeholder)));
      }
    }
  }
}