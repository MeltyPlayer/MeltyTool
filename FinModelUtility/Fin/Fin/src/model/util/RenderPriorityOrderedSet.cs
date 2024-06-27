using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using fin.data.dictionaries;
using fin.data.sets;

namespace fin.model.util {
  public class RenderPriorityOrderedSet<T> : IEnumerable<T> {
    // TODO: Optimize this somehow?
    private readonly OrderedHashSet<T> elements_ = new();

    private readonly AggregatedDictionary<T, uint> inversePriorityByElement_
        = new(Math.Min, new NullFriendlyDictionary<T, uint>());

    private readonly AggregatedDictionary<T, bool> isTransparentByElement_
        = new((existingValue, newValue) => existingValue || newValue,
              new NullFriendlyDictionary<T, bool>());

    public void Add(T item, uint inversePriority, bool isTransparent) {
      this.elements_.Add(item);
      this.inversePriorityByElement_[item] = inversePriority;
      this.isTransparentByElement_[item] = isTransparent;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<T> GetEnumerator()
      => this.elements_
             .OrderBy(value => this.inversePriorityByElement_[value])
             .ThenBy(value => this.isTransparentByElement_[value])
             .GetEnumerator();
  }
}