using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using fin.data.dictionaries;
using fin.data.sets;

namespace fin.model.util;

public sealed class RenderPriorityOrderedSet<T>
    : IEnumerable<(bool isTransparent, int index, uint inversePriority, T item)>
    where T : notnull {
  // TODO: Optimize this somehow?
  private readonly OrderedHashSet<T> elements_ = [];

  private readonly AggregatedDictionary<T, uint> inversePriorityByElement_
      = new(Math.Min, new SimpleDictionary<T, uint>());

  private readonly AggregatedDictionary<T, int> indexByElement_
      = new(Math.Min, new SimpleDictionary<T, int>());

  private readonly AggregatedDictionary<T, bool> isTransparentByElement_
      = new((existingValue, newValue) => existingValue || newValue,
            new SimpleDictionary<T, bool>());

  public void Add(T item, int index, uint inversePriority, bool isTransparent) {
    this.elements_.Add(item);
    this.indexByElement_[item] = index;
    this.inversePriorityByElement_[item] = inversePriority;
    this.isTransparentByElement_[item] = isTransparent;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(bool isTransparent, int index, uint inversePriority, T item)> GetEnumerator()
    => this.elements_
           .OrderBy(value => this.isTransparentByElement_[value])
           .ThenBy(value => this.inversePriorityByElement_[value])
           .ThenBy(value => this.indexByElement_[value])
           .Select(value => (this.isTransparentByElement_[value], this.indexByElement_[value], this.inversePriorityByElement_[value], value))
           .GetEnumerator();
}