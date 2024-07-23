using System;
using System.Collections.Generic;

namespace fin.data;

/// <summary>
///   View into a list that returns values in a random order.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ShuffledListView<T>(IReadOnlyList<T> impl) {
  // TODO: Implement an algorithm that "feels more random"
  public bool TryGetNext(out T value) {
    var count = impl.Count;

    if (count == 0) {
      value = default;
      return false;
    }

    value = impl[Random.Shared.Next(count)];
    return true;
  } 
}