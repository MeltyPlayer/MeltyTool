using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fin.data.parallel;

public static class ParallelUtil {
  public static Dictionary<TKey, TValue> ToDictionaryParallelized<
      T, TKey, TValue>(
      T[] inputs,
      Func<T, TKey> keySelector,
      Func<T, TValue> valueSelector) where TKey : notnull {
    var dictionary = new Dictionary<TKey, TValue>(inputs.Length);

    Parallel.For(
        0,
        inputs.Length,
        i => {
          var input = inputs[i];

          var key = keySelector(input);
          var value = valueSelector(input);

          dictionary[key] = value;
        });

    return dictionary;
  }
}