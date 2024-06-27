using System.Collections;

using Avalonia.Data.Converters;

namespace uni.ui.avalonia.util.converters;

public static class ListConverters {
  public static readonly IValueConverter HasAnyItems =
      new FuncValueConverter<IList?, bool>(x => (x?.Count ?? 0) > 0);
}