using System;
using System.Collections.Generic;
using System.Linq;

using fin.io;
using fin.util.asserts;

namespace fin.data.sets;

public interface ISetSplitter<TKey, out TValue> {
  TValue Matching(TKey key);
  IReadOnlyList<TValue> Matching(Func<TKey, bool> keyMatcher);
  IReadOnlyList<TValue> Remaining();
}

public class SetSplitter<TKey, TValue>(
    IEnumerable<TValue> values,
    Func<TValue, TKey> valueToKey,
    IEqualityComparer<TKey>? comparer)
    : ISetSplitter<TKey, TValue>
    where TKey : notnull {
  private IDictionary<TKey, TValue> impl_
      = values.ToDictionary(valueToKey, comparer);

  public TValue Matching(TKey key) {
    Asserts.True(this.impl_.Remove(key, out var value));
    return value!;
  }

  public IReadOnlyList<TValue> Matching(Func<TKey, bool> keyMatcher) {
    var matchingKeys = new List<TKey>();
    var matchingValues = new List<TValue>();
    foreach (var (key, value) in this.impl_) {
      if (keyMatcher(key)) {
        matchingKeys.Add(key);
        matchingValues.Add(value);
      }
    }

    foreach (var key in matchingKeys) {
      this.impl_.Remove(key);
    }

    return matchingValues;
  }

  public IReadOnlyList<TValue> Remaining() => this.impl_.Values.ToArray();
}

public static class SetSplitterExtensions {
  public static ISetSplitter<TKey, TValue> SplitBy<TKey, TValue>(
      this IEnumerable<TValue> values,
      Func<TValue, TKey> valueToKey,
      IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    => new SetSplitter<TKey, TValue>(values, valueToKey, comparer);

  public static ISetSplitter<string, TFile> SplitByName<TFile>(
      this IEnumerable<TFile> files) where TFile : IReadOnlyTreeFile
    => files.SplitBy(f => f.Name.ToString(), StringComparer.OrdinalIgnoreCase);

  public static IReadOnlyList<TFile> StartsWith<TFile>(
      this ISetSplitter<string, TFile> files,
      string prefix) where TFile : IReadOnlyTreeFile
    => files.Matching(n => n.StartsWith(prefix,
                                        StringComparison.OrdinalIgnoreCase));
}