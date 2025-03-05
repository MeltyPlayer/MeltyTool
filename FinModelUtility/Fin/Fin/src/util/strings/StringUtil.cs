using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NaturalSort.Extension;

namespace fin.util.strings;

public static class StringUtil {
  public static NaturalSortComparer NaturalSortInstance { get; }
    = new(StringComparison.OrdinalIgnoreCase);

  public static string Concat(
      IEnumerable<string> strs,
      string separator = "") {
    if (!strs.Any()) {
      return "";
    }

    var builder = new StringBuilder();

    builder.Append(strs.First());

    foreach (var str in strs.Skip(1)) {
      builder.Append(separator);
      builder.Append(str);
    }

    return builder.ToString();
  }
}