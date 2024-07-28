using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HaloWarsTools {
  public class LazyValueCache {
    private Dictionary<string, dynamic> CachedValues = new();

    public bool Contains(string key) {
      return this.CachedValues.ContainsKey(key);
    }

    // Lazy property loading system. Anything can call Get<T> with a loader function. The first time
    // it's called, the loader function will be invoked and the return value will be stored in CachedValues.
    // Subsequent calls to Get<T> will look the value up in CachedValues instead of invoking the loader function.
    //
    // Example:
    // public string Value => ValueCache.Get(() => File.ReadAllText("file.txt"));
    public T Get<T>(Func<T> loadFunction, [CallerMemberName] string key = null) {
      if (string.IsNullOrEmpty(key)) {
        throw new Exception($"No key passed to {nameof(LazyValueCache)}.{nameof(this.Get)} call");
      }

      if (!this.CachedValues.TryGetValue(key, out dynamic value)) {
        value = loadFunction();
        Set<T>(key, value);
      }

      return value;
    }

    public void Set<T>(string key, T value) {
      this.CachedValues.Add(key, value);
    }
  }
}