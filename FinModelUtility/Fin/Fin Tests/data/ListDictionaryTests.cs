using System.Collections;
using System.Collections.Generic;
using System.Linq;

using fin.data.dictionaries;
using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.data;

public class ListDictionaryTests {
  [Test]
  public void TestClear() {
    var impl = new ListDictionary<string?, string>();
    Assert.AreEqual(0, impl.Count);

    impl.Add("foo", "a");
    impl.Add("foo", "b");
    impl.Add("foo", "c");
    Assert.AreEqual(3, impl.Count);

    impl.Add("bar", "1");
    impl.Add("bar", "2");
    impl.Add("bar", "3");
    Assert.AreEqual(6, impl.Count);

    impl.Add(null, "x");
    impl.Add(null, "y");
    impl.Add(null, "z");
    Assert.AreEqual(9, impl.Count);

    impl.Clear();
    Assert.AreEqual(0, impl.Count);
  }

  [Test]
  public void TestTryGetList() {
    var impl = new ListDictionary<string?, string>();
    Assert.AreEqual(0, impl.Count);

    Assert.AreEqual(false, impl.TryGetList("foo", out _));
    Assert.AreEqual(false, impl.TryGetList("bar", out _));
    Assert.AreEqual(false, impl.TryGetList(null, out _));

    impl.Add("foo", "a");
    impl.Add("foo", "b");
    impl.Add("foo", "c");
    Assert.AreEqual(3, impl.Count);

    impl.Add("bar", "1");
    impl.Add("bar", "2");
    impl.Add("bar", "3");
    Assert.AreEqual(6, impl.Count);

    impl.Add(null, "x");
    impl.Add(null, "y");
    impl.Add(null, "z");
    Assert.AreEqual(9, impl.Count);

    Assert.AreEqual(true, impl.TryGetList("foo", out var fooList));
    Asserts.SequenceEqual(["a", "b", "c"], fooList!);

    Assert.AreEqual(true, impl.TryGetList("bar", out var barList));
    Asserts.SequenceEqual(["1", "2", "3"], barList!);

    Assert.AreEqual(true, impl.TryGetList(null, out var nullList));
    Asserts.SequenceEqual(["x", "y", "z"], nullList!);
  }

  [Test]
  public void TestEnumeratorLinq() {
    var impl = new ListDictionary<string?, string>();
    Assert.AreEqual(0, impl.Count);

    impl.Add("foo", "a");
    impl.Add("foo", "b");
    impl.Add("foo", "c");
    Assert.AreEqual(3, impl.Count);

    impl.Add("bar", "1");
    impl.Add("bar", "2");
    impl.Add("bar", "3");
    Assert.AreEqual(6, impl.Count);

    impl.Add(null, "x");
    impl.Add(null, "y");
    impl.Add(null, "z");
    Assert.AreEqual(9, impl.Count);

    var actualValues = impl.GetPairs().ToArray();

    Assert.AreEqual(true, impl.TryGetList("foo", out var fooList));
    Assert.AreEqual(true, impl.TryGetList("bar", out var barList));
    Assert.AreEqual(true, impl.TryGetList(null, out var nullList));

    Asserts.SequenceEqual([
                              ("bar", barList!),
                              ("foo", fooList!),
                              (null, nullList!),
                          ],
                          actualValues);
  }
}