using System.Collections.Generic;

namespace fin.data.fuzzy;

public interface IFuzzySearchResult<out T> {
  T Data { get; }
  int ChangeDistance { get; }
  float Similarity { get; }
}

public interface IFuzzySearchDictionary<T> {
  void Add(string keyword, T data);

  IEnumerable<IFuzzySearchResult<T>> Search(
      string filterText,
      float minMatchPercentage);
}


public interface IFuzzyNode<T> {
  T Data { get; set; }
  float Similarity { get; }
  int ChangeDistance { get; }

  IReadOnlySet<string> Keywords { get; }

  IFuzzyNode<T>? Parent { get; }
  IReadOnlyList<IFuzzyNode<T>> Children { get; }

  IFuzzyNode<T> AddChild(T data);
}

public interface IFuzzyFilterTree<T> {
  IFuzzyNode<T> Root { get; }

  void Reset();

  void Filter(
      string keyword,
      float minMatchPercentage);
}