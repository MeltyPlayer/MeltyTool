using Avalonia;
using Avalonia.Controls;

using Material.Icons;

namespace uni.ui.avalonia.common.icons;

public partial class MaterialIconBadge : UserControl {
  public MaterialIconBadge() {
    this.InitializeComponent();
  }

  public static readonly StyledProperty<MaterialIconKind> KindProperty
      = AvaloniaProperty.Register<MaterialIconBadge, MaterialIconKind>(
          nameof(Kind),
          defaultValue: MaterialIconKind.AnimationOutline);

  public MaterialIconKind Kind {
    get => this.GetValue(KindProperty);
    set => this.SetValue(KindProperty, value);
  }

  public static readonly StyledProperty<int> CountProperty
      = AvaloniaProperty.Register<MaterialIconBadge, int>(nameof(Count));

  public int Count {
    get => this.GetValue(CountProperty);
    set => this.SetValue(CountProperty, value);
  }
}