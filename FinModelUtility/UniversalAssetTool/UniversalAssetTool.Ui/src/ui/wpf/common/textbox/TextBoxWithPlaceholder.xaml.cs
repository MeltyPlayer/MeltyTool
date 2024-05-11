using System.Windows.Controls;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

namespace uni.ui.wpf.common.textbox {
  /// <summary>
  /// Interaction logic for TextBoxWithPlaceholder.xaml
  /// </summary>
  public partial class TextBoxWithPlaceholder : UserControl, ITextBox {
    private readonly TextBoxWithPlaceholderViewModel viewModel_ = new();

    public TextBoxWithPlaceholder() {
      InitializeComponent();
      this.DataContext = this.viewModel_;
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

    private void OnTextChanged_(object sender, TextChangedEventArgs args)
      => this.placeholderLabel_.Visibility
          = this.impl_.Text != ""
              ? Visibility.Hidden
              : Visibility.Visible;
  }

  public partial class TextBoxWithPlaceholderViewModel : ObservableObject {
    [ObservableProperty]
    protected string placeholder = "Search...";
  }
}