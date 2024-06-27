using System;
using System.Collections.Generic;

namespace fin.util.lists;

public static class ListUtil {
  public static List<T> OfLength<T>(int length) {
    var list = new List<T>(length);
    for (var i = 0; i < length; ++i) {
      list.Add(default!);
    }

    return list;
  }

  public static bool TryFindFirst<T>(IReadOnlyList<T> data,
                                     T value,
                                     out int index) {
    for (var i = 0; i < data.Count; i++) {
      var dataValue = data[i];
      if (dataValue?.Equals(value) ?? (dataValue == null && value == null)) {
        index = i;
        return true;
      }
    }

    index = -1;
    return false;
  }

  public static int AssertFindFirst<T>(IReadOnlyList<T> data, T value) {
    if (ListUtil.TryFindFirst(data, value, out var index)) {
      return index;
    }

    throw new Exception($"Failed to find value \"{value}\" in array.");
  }

  public static bool RemoveFromFirstIfNotInSecond<T>(
      IList<T> first,
      IList<T> second)
    => RemoveWhere(first,
                   tFromFirst => !second.Contains(tFromFirst));

  public static bool RemoveWhere<T>(IList<T> impl, Func<T, bool> removeFunc) {
    var indices = new LinkedList<int>();
    for (var i = 0; i < impl.Count; ++i) {
      if (removeFunc(impl[i])) {
        indices.AddFirst(i);
      }
    }

    foreach (var i in indices) {
      impl.RemoveAt(i);
    }

    return indices.Count > 0;
  }

  public static IList<T> FromParams<T>(params T[] data) => data;

  public static IList<T> From<T>(T first, params T[] rest) {
    var all = new T[1 + rest.Length];
    all[0] = first;
    for (var i = 0; i < rest.Length; ++i) {
      all[1 + i] = rest[i];
    }
    return all;
  }


  public static IReadOnlyList<T> ReadonlyFromParams<T>(params T[] data)
    => data;

  public static IReadOnlyList<T> ReadonlyFrom<T>(T first, params T[] rest) {
    var all = new T[1 + rest.Length];
    all[0] = first;
    for (var i = 0; i < rest.Length; ++i) {
      all[1 + i] = rest[i];
    }
    return all;
  }

  public static IReadOnlyList<T> ReadonlyFrom<T>(
      T first,
      T second,
      params T[] rest) {
    var all = new T[2 + rest.Length];
    all[0] = first;
    all[1] = second;
    for (var i = 0; i < rest.Length; ++i) {
      all[2 + i] = rest[i];
    }
    return all;
  }

  public static IReadOnlyList<T> ReadonlyConcat<T>(
      params IReadOnlyList<T>?[] lists) {
    var totalCount = 0;
    foreach (var list in lists) {
      totalCount += list?.Count ?? 0;
    }

    var all = new T[totalCount];
    var p = 0;
    foreach (var list in lists) {
      if (list != null) {
        for (var i = 0; i < list.Count; ++i) {
          all[p++] = list[i];
        }
      }
    }

    return all;
  }
}