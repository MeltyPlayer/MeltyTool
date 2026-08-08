using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

using Material.Icons;

using ReactiveUI.Primitives;

namespace fin.ui.avalonia.buttons;

public partial class ContrastableIconButton : UserControl {
  public static readonly StyledProperty<MaterialIconKind> IconProperty =
      AvaloniaProperty.Register<ContrastableIconButton, MaterialIconKind>(
          nameof(Icon));

  public MaterialIconKind Icon {
    get => this.GetValue(IconProperty);
    set => this.SetValue(IconProperty, value);
  }


  public static readonly StyledProperty<string> TooltipProperty =
      AvaloniaProperty.Register<ContrastableIconButton, string>(
          nameof(Tooltip));

  public string Tooltip {
    get => this.GetValue(TooltipProperty);
    set => this.SetValue(TooltipProperty, value);
  }


  public static new readonly StyledProperty<bool> IsEnabledProperty =
      AvaloniaProperty.Register<ContrastableIconButton, bool>(
          nameof(IsEnabled),
          true);

  public new bool IsEnabled {
    get => this.GetValue(IsEnabledProperty);
    set => this.SetValue(IsEnabledProperty, value);
  }


  public static readonly StyledProperty<bool> HighContrastProperty =
      AvaloniaProperty.Register<ContrastableIconButton, bool>(
          nameof(HighContrast));

  public bool HighContrast {
    get => this.GetValue(HighContrastProperty);
    set => this.SetValue(HighContrastProperty, value);
  }

  public IObservable<IBrush> ΔForeground
    => this.GetObservable(IsEnabledProperty)
           .Select(isEnabled => isEnabled
                       ? SolidColorBrush.Parse("White")
                       : SolidColorBrush.Parse("Gray"));

  public event EventHandler<RoutedEventArgs> Click;

  public ContrastableIconButton() => this.InitializeComponent();

  private void Button_OnClick(object? sender, RoutedEventArgs e)
    => this.Click?.Invoke(sender, e);
}