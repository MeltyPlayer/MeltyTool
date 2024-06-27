using System;
using System.Linq;

namespace fin.util.array;

public static class Arrays {
  public static T[] From<T>(int count, Func<T> instantiator) {
    var array = new T[count];
    for (var i = 0; i < count; ++i) {
      array[i] = instantiator();
    }
    return array;
  }

  public static T[] Concat<T>(params T[][] arrays) {
    var totalSize = arrays.Select(array => array.Length)
                          .Aggregate((lhs, rhs) => lhs + rhs);

    var totalArray = new T[totalSize];
    var i = 0;
    foreach (var array in arrays) {
      foreach (var t in array) {
        totalArray[i++] = t;
      }
    }

    return totalArray;
  }
}