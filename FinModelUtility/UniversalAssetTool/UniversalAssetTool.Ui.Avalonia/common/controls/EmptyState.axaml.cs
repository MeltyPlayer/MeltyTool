using Avalonia;
using Avalonia.Controls;

using Material.Icons;

namespace uni.ui.avalonia.common.controls {
  public partial class EmptyState : UserControl {
    public EmptyState() {
      this.InitializeComponent();
    }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<EmptyState, string>(
            nameof(Text),
            defaultValue: "Nothing to display.");

    public string Text {
      get => this.GetValue(TextProperty);
      set => this.SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<EmptyState, MaterialIconKind>(
            nameof(Icon),
            defaultValue: MaterialIconKind.Clippy);

    public MaterialIconKind Icon {
      get => this.GetValue(IconProperty);
      set => this.SetValue(IconProperty, value);
    }
  }
}