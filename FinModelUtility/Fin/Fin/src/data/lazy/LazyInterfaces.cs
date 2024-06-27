using fin.data.dictionaries;

namespace fin.data.lazy;

public interface ILazyDictionary<TKey, TValue>
    : IFinDictionary<TKey, TValue>;

public interface ILazyArray<T> : ILazyDictionary<int, T>;