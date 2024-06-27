using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.data.lazy;

public class LazyArrayTests {
  [Test]
  public void TestWithKeyAndValueHandler() {
    var invokeCount = 0;
    var lazyReverseMap = new LazyArray<string>(2,
                                               i => {
                                                 invokeCount++;
                                                 return $"foo{i}";
                                               });

    Assert.AreEqual(2, lazyReverseMap.Count);
    Assert.AreEqual(0, invokeCount);

    Assert.AreEqual("foo1", lazyReverseMap[1]);
    Assert.AreEqual(2, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);

    // Reuses existing value
    Assert.AreEqual("foo1", lazyReverseMap[1]);
    Assert.AreEqual(2, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);
  }

  [Test]
  public void TestEnumerators() {
    var lazyReverseMap = new LazyArray<string>(10, i => $"foo{i}");

    Assert.AreEqual("foo3", lazyReverseMap[3]);
    Assert.AreEqual("foo1", lazyReverseMap[1]);
    Assert.AreEqual("foo5", lazyReverseMap[5]);

    Asserts.SequenceEqual(lazyReverseMap.Keys, new[] { 1, 3, 5 });
    Asserts.SequenceEqual(lazyReverseMap.Values, new[] { "foo1", "foo3", "foo5" });
  }
}