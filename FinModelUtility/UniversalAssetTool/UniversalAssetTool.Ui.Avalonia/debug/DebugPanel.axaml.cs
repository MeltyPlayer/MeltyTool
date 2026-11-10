using System;
using System.Linq;
using System.Reactive.Linq;

using Avalonia.Controls;

using fin.ui.avalonia.util;
using fin.ui.rendering;

using ReactiveUI;

namespace uni.ui.avalonia.debug;

public class DebugPanelRow(
    string key,
    IObservable<string> Δvalue,
    bool indented) {
  public string Key => key;
  public IObservable<string> ΔValue => Δvalue;
  public bool Indented => indented;

  public static implicit operator DebugPanelRow(
      (string Key, string Value, bool Indented) tuple)
    => new(tuple.Key, Observable.Return(tuple.Value), tuple.Indented);


  public static implicit operator DebugPanelRow(
      (string Key, IObservable<string> ΔValue, bool Indented) tuple)
    => new(tuple.Key, tuple.ΔValue, tuple.Indented);
}

public sealed partial class DebugPanel : UserControl {
  public DebugPanel() {
    var debugServiceViewModel = DebugService.ViewModel;

    var rows = new DebugPanelRow[] {
        ("Vendor", debugServiceViewModel.WhenAnyValue(x => x.VendorName), false),
        ("Renderer", debugServiceViewModel.WhenAnyValue(x => x.RendererName), false),
        ("Render graph element count",
         debugServiceViewModel.WhenAnyValue(x => x.RenderGraphElementCount)
                              .Select(x => x.ToString()),
         false),
        ("Model Count",
         debugServiceViewModel.WhenAnyValue(x => x.ModelCount)
                              .Select(x => x.ToString()),
         false),
        ("Material count",
         debugServiceViewModel.WhenAnyValue(x => x.MaterialCount)
                              .Select(x => x.ToString()),
         false),
        ("Opaque material count",
         debugServiceViewModel.WhenAnyValue(x => x.OpaqueMaterialCount)
                              .Select(x => x.ToString()),
         true),
        ("Transparent material count",
         debugServiceViewModel.WhenAnyValue(x => x.TransparentMaterialCount)
                              .Select(x => x.ToString()),
         true),
        ("Program count",
         debugServiceViewModel.WhenAnyValue(x => x.ProgramCount)
                              .Select(x => x.ToString()),
         false),
        ("Vertex shader count",
         debugServiceViewModel.WhenAnyValue(x => x.VertexShaderCount)
                              .Select(x => x.ToString()),
         true),
        ("Fragment shader count",
         debugServiceViewModel.WhenAnyValue(x => x.FragmentShaderCount)
                              .Select(x => x.ToString()),
         true),
        ("Fin texture count",
         debugServiceViewModel.WhenAnyValue(x => x.FinTextureCount)
                              .Select(x => x.ToString()),
         false),
        ("OpenGL texture count",
         debugServiceViewModel.WhenAnyValue(x => x.OpenGlTextureCount)
                              .Select(x => x.ToString()),
         true),
        ("Sampler count",
         debugServiceViewModel.WhenAnyValue(x => x.SamplerCount)
                              .Select(x => x.ToString()),
         true),
    };

    var grid = new Grid();
    grid.ColumnDefinitions.AddRange([
        new ColumnDefinition(GridLength.Auto),
        new ColumnDefinition(GridLength.Star)
    ]);

    for (var i = 0; i < rows.Length; ++i) {
      grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
    }


    grid.Children.AddRange(rows.SelectMany((row, i) => {
      var keyTextBlock = new TextBlock { Text = row.Key };
      keyTextBlock.AddClass("regular");
      if (row.Indented) {
        keyTextBlock.AddClass("indented");
      }

      var keyBorder = new Border {
          Child = keyTextBlock,
      };
      Grid.SetColumn(keyBorder, 0);
      Grid.SetRow(keyBorder, i);

      var valueTextBlock = new SelectableTextBlock();
      valueTextBlock.AddClass("regular");

      valueTextBlock.Bind(TextBlock.TextProperty, row.ΔValue);

      var valueBorder = new Border {
          Child = valueTextBlock,
      };
      Grid.SetColumn(valueBorder, 1);
      Grid.SetRow(valueBorder, i);

      return new Control[] { keyBorder, valueBorder };
    }));

    InitializeComponent();

    this.Host.Child = grid;
  }
}