using System.Collections;
using System.Collections.Generic;
using System.Linq;

using fin.data.dictionaries;

namespace fin.model.util {
  public class RenderPriorityOrderedSet<T> : IEnumerable<T> {
    // TODO: Optimize this with something like a minmap?
    private readonly SetDictionary<T, (int addOrder, uint inversePriority, bool
        isTransparent)> impl_ = new();

    public void Add(T item, uint inversePriority, bool isTransparent)
      => this.impl_.Add(item,
                        (this.impl_.Count, inversePriority, isTransparent));

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<T> GetEnumerator()
      => this.impl_
             .OrderBy(pair => pair.Value.Select(t => t.inversePriority)
                                  .Order()
                                  .First())
             .ThenBy(pair => pair.Value.Select(t => t.isTransparent)
                                 .Order()
                                 .First())
             .Select(pair => pair.Key)
             .GetEnumerator();
  }
}