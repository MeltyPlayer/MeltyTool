using System;

using Avalonia;
using Avalonia.Controls;

namespace uni.ui.avalonia.common.textboxes {
  public partial class AutocompleteTextbox : UserControl, ITextBox {
    public AutocompleteTextbox() {
      InitializeComponent();
    }

    public string? Text {
      get => this.impl_.Text;
      set => this.impl_.Text = value;
    }

    public event EventHandler<TextChangedEventArgs> TextChanged {
      add => this.impl_.TextChanged += value;
      remove => this.impl_.TextChanged -= value;
    }

    public static readonly StyledProperty<string> PlaceholderProperty =
        AvaloniaProperty.Register<AutocompleteTextbox, string>(
            nameof(Placeholder),
            defaultValue: "Search...");

    public string Placeholder {
      get => this.GetValue(PlaceholderProperty);
      set => this.SetValue(PlaceholderProperty, value);
    }

    public static readonly StyledProperty<AutoCompleteFilterMode>
        FilterModeProperty =
            AvaloniaProperty
                .Register<AutocompleteTextbox, AutoCompleteFilterMode>(
                    nameof(Placeholder),
                    defaultValue: AutoCompleteFilterMode.ContainsOrdinal);

    public AutoCompleteFilterMode FilterMode {
      get => this.GetValue(FilterModeProperty);
      set => this.SetValue(FilterModeProperty, value);
    }

    private void OnTextChanged_(object sender, TextChangedEventArgs args)
      => this.placeholderLabel_.IsVisible = this.Text == "";
  }
}