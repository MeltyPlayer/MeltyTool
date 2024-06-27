using System.Collections;

using Avalonia.Data.Converters;

namespace uni.ui.avalonia.util.converters {
  public static class IntConverters {
    public static readonly IValueConverter IsNot0 =
        new FuncValueConverter<int?, bool>(x => (x ?? 0) != 0);
  }
}