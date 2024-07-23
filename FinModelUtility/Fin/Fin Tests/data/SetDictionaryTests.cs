using System.Collections.Generic;
using System.Linq;

using fin.data.dictionaries;
using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.data;

public class SetDictionaryTests {
  [Test]
  public void TestAdd() {
    var impl = new SetDictionary<string, string>();
    impl.Add("foo", "bar");
    impl.Add("foo", "goo");

    Assert.AreEqual(2, impl.Count);
    Assert.IsTrue(impl.TryGetSet("foo", out var outSet));
    Assert.AreEqual(outSet!, impl["foo"]);
    Asserts.SequenceEqual<IEnumerable<string>>(outSet!.Order(),
                                               new[] { "bar", "goo" });
  }

  [Test]
  public void TestClear() {
    var impl = new SetDictionary<string, string>();
    impl.Add("foo", "bar");
    impl.Add("foo", "goo");

    impl.Clear();

    Assert.AreEqual(0, impl.Count);
    Assert.IsFalse(impl.TryGetSet("foo", out _));
  }
}