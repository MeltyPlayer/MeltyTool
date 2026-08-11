using Avalonia;
using Avalonia.Controls;

namespace fin.ui.avalonia.styles;

public sealed class HeaderResources : ResourceDictionary {
  public const int MAX_SIZE_INDEX = 1;
  public const int MIN_SIZE_INDEX = 4;

  public double[] FontSizes { get; }
  public Thickness[] Paddings { get; }

  public static HeaderResources Instance {
    get => field ??= new HeaderResources();
  }

  public HeaderResources() {
    this.FontSizes = new double[MIN_SIZE_INDEX];
    this.Paddings = new Thickness[MIN_SIZE_INDEX];

    for (var i = MAX_SIZE_INDEX; i <= MIN_SIZE_INDEX; i++) {
      var fontSize = 13 + (MIN_SIZE_INDEX - MAX_SIZE_INDEX - i) * 2;
      
      var topPadding = fontSize * .5;
      var padding = new Thickness(0, topPadding, 0, 0);

      this.FontSizes[i - 1] = fontSize;
      this.Paddings[i - 1] = padding;

      this[$"Header{i}FontSize"] = (double) fontSize;
      this[$"Header{i}Padding"] = padding;
    }
  }
}