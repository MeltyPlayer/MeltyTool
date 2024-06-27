using Avalonia.Data.Converters;

using fin.util.strings;

namespace uni.ui.avalonia.util.converters;

public static class StringConverters {
  public static readonly IValueConverter StartsWithSlashes =
      new FuncValueConverter<string?, bool>(
          s => s?.StartsWith("//") ?? false);

  public static readonly IValueConverter SubstringAfterSlashes =
      new FuncValueConverter<string?, string?>(
          s => s?.SubstringAfter("//"));
}