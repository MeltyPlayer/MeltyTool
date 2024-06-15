using System.Collections.Generic;

using fin.io;

namespace fin.util.sets {
  public static class HashSetExtensions {
    public static HashSet<T> AsSet<T>(this T value) => [value];

    public static HashSet<IReadOnlyGenericFile> AsFileSet<T>(this T value)
        where T : IReadOnlyGenericFile
      => [value];

    public static bool Add<T>(this ISet<T> set, IEnumerable<T> values) {
      var didAddAny = false;
      foreach (var value in values) {
        var didAdd = set.Add(value);
        didAddAny = didAddAny || didAdd;
      }

      return didAddAny;
    }
  }
}