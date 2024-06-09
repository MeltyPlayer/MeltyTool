using System;
using System.Linq;

using fin.util.asserts;

using NUnit.Framework;

namespace fin.util.enumerables {
  public class EnumeratorExtensionsTests {
    [Test]
    public void TestTryMoveNextEmpty() {
      var emptyEnumerator = Enumerable.Empty<int>().ToEnumerator();

      Asserts.False(emptyEnumerator.TryMoveNext(out _));
      Asserts.False(emptyEnumerator.TryMoveNext(out _));
      Asserts.False(emptyEnumerator.TryMoveNext(out _));
    }

    [Test]
    public void TestTryMoveNext() {
      var enumerator = new[] { 1, 2, 3 }.ToEnumerator();

      Asserts.True(enumerator.TryMoveNext(out var first));
      Asserts.Equal(1, first);

      Asserts.True(enumerator.TryMoveNext(out var second));
      Asserts.Equal(2, second);

      Asserts.True(enumerator.TryMoveNext(out var third));
      Asserts.Equal(3, third);

      Asserts.False(enumerator.TryMoveNext(out _));
      Asserts.False(enumerator.TryMoveNext(out _));
      Asserts.False(enumerator.TryMoveNext(out _));
    }
  }
}