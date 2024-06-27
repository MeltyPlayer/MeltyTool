using System;
using System.Collections.Generic;
using System.Linq;

using fin.util.asserts;
using fin.util.strings;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.data.lazy;

public class LazyDictionaryTests {
  [Test]
  public void TestWithKeyAndValueHandler() {
    var invokeCount = 0;
    var lazyReverseMap = new LazyDictionary<string, string>(inStr => {
      invokeCount++;
      return inStr.Reverse();
    });

    Assert.AreEqual(0, lazyReverseMap.Count);
    Assert.AreEqual(0, invokeCount);

    Assert.AreEqual("esrever", lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);

    // Reuses existing value
    Assert.AreEqual("esrever", lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);
  }

  [Test]
  public void TestWithDictionaryKeyAndValueHandler() {
    var invokeCount = 0;
    LazyDictionary<string, string>? lazyReverseMap = null;
    lazyReverseMap = new LazyDictionary<string, string>((dict, inStr) => {
      Assert.AreSame(lazyReverseMap, dict);
      invokeCount++;
      return inStr.Reverse();
    });

    Assert.AreEqual(0, lazyReverseMap.Count);
    Assert.AreEqual(0, invokeCount);

    Assert.AreEqual("esrever", lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);

    // Reuses existing value
    Assert.AreEqual("esrever", lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);
  }

  [Test]
  public void TestSettingValuesDirectly() {
    var lazyReverseMap = new LazyDictionary<string, string>(
        _ => throw new NotImplementedException());

    Assert.AreEqual(0, lazyReverseMap.Count);

    lazyReverseMap["reverse"] = "esrever";
    Assert.AreEqual("esrever", lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
  }

  [Test]
  public void TestClear() {
    var invokeCount = 0;
    var lazyReverseMap = new LazyDictionary<string, string>(inStr => {
      invokeCount++;
      return inStr.Reverse();
    });

    Assert.AreEqual(0, lazyReverseMap.Count);
    Assert.AreEqual(0, invokeCount);

    Assert.AreEqual("esrever", lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);

    lazyReverseMap.Clear();
    Assert.AreEqual(0, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);

    Assert.AreEqual("esrever", lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(2, invokeCount);
  }

  [Test]
  public void TestContainsKey() {
    var lazyReverseMap =
        new LazyDictionary<string, string>(inStr => inStr.Reverse());

    Assert.AreEqual(false, lazyReverseMap.ContainsKey("reverse"));

    Assert.AreEqual("esrever", lazyReverseMap["reverse"]);
    Assert.AreEqual(true, lazyReverseMap.ContainsKey("reverse"));

    lazyReverseMap.Clear();
    Assert.AreEqual(false, lazyReverseMap.ContainsKey("reverse"));
  }

  [Test]
  public void TestEnumerators() {
    var lazyReverseMap =
        new LazyDictionary<string, string>(inStr => inStr.Reverse());

    Assert.AreEqual("oof", lazyReverseMap["foo"]);
    Assert.AreEqual("rab", lazyReverseMap["bar"]);
    Assert.AreEqual("oog", lazyReverseMap["goo"]);

    Asserts.SequenceEqual<IEnumerable<string>>(lazyReverseMap.Keys.Order(),
                                               new[] { "bar", "foo", "goo" });
    Asserts.SequenceEqual<IEnumerable<string>>(lazyReverseMap.Values.Order(),
                                               new[] { "oof", "oog", "rab", });
  }
}