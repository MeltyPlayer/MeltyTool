using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;

namespace uni.ui.wpf.common.textbox {
  /// <summary>
  /// Interaction logic for WatermarkTextBox.xaml
  /// </summary>
  public partial class TextBoxWithPlaceholder : UserControl, ITextBox {
    private readonly TextBoxWithPlaceholderViewModel viewModel_;

    public TextBoxWithPlaceholder(TextBoxWithPlaceholderViewModel viewModel) {
      InitializeComponent();
      this.viewModel_ = viewModel;
    }

    public string Text {
      get => this.impl_.Text;
      set => this.impl_.Text = value;
    }

    public event TextChangedEventHandler TextChanged {
      add => this.impl_.TextChanged += value;
      remove => this.impl_.TextChanged -= value;
    }

    public int CaretIndex {
      get => this.impl_.CaretIndex;
      set => this.impl_.CaretIndex = value;
    }

    public int MaxLength {
      get => this.impl_.MaxLines;
      set => this.impl_.MaxLines = value;
    }

    public int MaxLines {
      get => this.impl_.MaxLines;
      set => this.impl_.MaxLines = value;
    }

    public CharacterCasing CharacterCasing {
      get => this.impl_.CharacterCasing;
      set => this.impl_.CharacterCasing = value;
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
            new PropertyChangedEventArgs(nameof(this.Placeholder)));
      }
    }
  }
}