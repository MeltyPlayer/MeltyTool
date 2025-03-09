using System.Collections.Generic;
using System.Linq;

using fin.util.enumerables;

namespace fin.data.indexable;

public interface IIndexedDescendantsSet {
  void AddChild(int parent, int child);
  bool TryGetDescendants(int parent, out HashSet<int> descendants);
}

public class IndexedDescendantsSet : IIndexedDescendantsSet {
  private readonly IndexedDictionary<int> parents_ = new();
  private readonly IndexedSetDictionary<int> descendants_ = new();

  public void AddChild(int parent, int child) {
    this.parents_[child] = parent;

    IEnumerable<int> descendants = child.Yield();
    if (this.TryGetDescendants(child, out var childChildren)) {
      descendants = descendants.Concat(childChildren);
    }

    do {
      var dst = this.descendants_.GetOrCreateSet(parent);
      foreach (var descendant in descendants) {
        dst.Add(descendant);
      }
    } while (this.parents_.TryGetValue(parent, out parent));
  }

  public bool TryGetDescendants(int parent, out HashSet<int> descendants)
    => this.descendants_.TryGetSet(parent, out descendants);
}