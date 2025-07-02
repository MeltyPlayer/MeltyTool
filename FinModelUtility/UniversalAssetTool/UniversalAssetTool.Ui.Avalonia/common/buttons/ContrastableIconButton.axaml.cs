using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using Material.Icons;

namespace uni.ui.avalonia.common.buttons;

public partial class ContrastableIconButton : UserControl {
  public static readonly StyledProperty<MaterialIconKind> IconProperty =
      AvaloniaProperty.Register<ExportAssetButton, MaterialIconKind>(
          nameof(Icon));

  public MaterialIconKind Icon {
    get => this.GetValue(IconProperty);
    set => this.SetValue(IconProperty, value);
  }


  public static readonly StyledProperty<string> TooltipProperty =
      AvaloniaProperty.Register<ExportAssetButton, string>(
          nameof(Tooltip));

  public string Tooltip {
    get => this.GetValue(TooltipProperty);
    set => this.SetValue(TooltipProperty, value);
  }


  public static readonly StyledProperty<bool> IsEnabledProperty =
      AvaloniaProperty.Register<ExportAssetButton, bool>(
          nameof(IsEnabled));

  public bool IsEnabled {
    get => this.GetValue(IsEnabledProperty);
    set => this.SetValue(IsEnabledProperty, value);
  }


  public static readonly StyledProperty<bool> HighContrastProperty =
      AvaloniaProperty.Register<ExportAssetButton, bool>(
          nameof(HighContrast));

  public bool HighContrast {
    get => this.GetValue(HighContrastProperty);
    set => this.SetValue(HighContrastProperty, value);
  }

  public event EventHandler<RoutedEventArgs> Click;

  public ContrastableIconButton() => this.InitializeComponent();

  private void Button_OnClick(object? sender, RoutedEventArgs e)
    => this.Click?.Invoke(sender, e);
}