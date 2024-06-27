using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using schema.binary;
using schema.binary.attributes;
using schema.util.enumerables;
using schema.util.sequences;

namespace fin.schema.sequences;

[BinarySchema]
public partial class ConsecutiveLists2<T1, T2>
    : ISequence<ConsecutiveLists2<T1, T2>, (T1 First, T2 Second)>
    where T1 : IBinaryConvertible, new()
    where T2 : IBinaryConvertible, new() {
  [RSequenceLengthSource(nameof(Count))]
  private readonly List<T1> list1_;

  [RSequenceLengthSource(nameof(Count))]
  private readonly List<T2> list2_;

  public ConsecutiveLists2() : this([], []) { }

  private ConsecutiveLists2(List<T1> list1, List<T2> list2) {
    this.list1_ = list1;
    this.list2_ = list2;
  }

  public (T1, T2) GetDefault() => (new(), new());

  [Skip]
  public int Count => this.list1_.Count;

  public void Clear() {
    this.list1_.Clear();
    this.list2_.Clear();
  }

  public void ResizeInPlace(int newLength) {
    SequencesUtil.ResizeSequenceInPlace(this.list1_, newLength);
    SequencesUtil.ResizeSequenceInPlace(this.list2_, newLength);
  }

  ConsecutiveLists2<T1, T2>
      IReadOnlySequence<ConsecutiveLists2<T1, T2>, (T1 First, T2 Second)>.
      CloneWithNewLength(int newLength) {
    var additionalLength = Math.Max(this.Count - newLength, 0);
    return new(this.list1_.Resized(newLength)
                   .Concat(Enumerable.Repeat(new T1(), additionalLength))
                   .ToList(),
               this.list2_.Resized(newLength)
                   .Concat(Enumerable.Repeat(new T2(), additionalLength))
                   .ToList());
  }

  [Skip]
  public (T1 First, T2 Second) this[int index] {
    get => (this.list1_[index], this.list2_[index]);
    set => (this.list1_[index], this.list2_[index]) = value;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(T1 First, T2 Second)> GetEnumerator() => this.list1_
      .Select((t, i) => (t, this.list2_[i]))
      .GetEnumerator();
}