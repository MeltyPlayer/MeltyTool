using System;

using Avalonia;
using Avalonia.Controls;

namespace uni.ui.avalonia.common.textboxes {
  public partial class PlaceholderTextbox : UserControl, IMultiLineTextBox {
    public PlaceholderTextbox() {
      this.InitializeComponent();
    }

    public string? Text {
      get => this.impl_.Text;
      set => this.impl_.Text = value;
    }

    public event EventHandler<TextChangedEventArgs> TextChanged {
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

    public static readonly StyledProperty<string> PlaceholderProperty =
        AvaloniaProperty.Register<PlaceholderTextbox, string>(
            nameof(Placeholder),
            defaultValue: "Search...");

    public string Placeholder {
      get => this.GetValue(PlaceholderProperty);
      set => this.SetValue(PlaceholderProperty, value);
    }

    private void OnTextChanged_(object sender, TextChangedEventArgs args)
      => this.placeholderLabel_.IsVisible = this.Text == "";
  }
}