using System;
using System.Threading.Tasks;

using fin.util.strings;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.data.lazy;

public class LazyAsyncDictionaryTests {
  [Test]
  public async Task TestWithKeyAndValueHandler() {
    var invokeCount = 0;
    var lazyReverseMap = new LazyDictionary<string, Task<string>>(inStr => {
      invokeCount++;
      return Task.FromResult(inStr.Reverse());
    });

    Assert.AreEqual(0, lazyReverseMap.Count);
    Assert.AreEqual(0, invokeCount);

    Assert.AreEqual("esrever", await lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);

    // Reuses existing value
    Assert.AreEqual("esrever", await lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);
  }

  [Test]
  public async Task TestWithDictionaryKeyAndValueHandler() {
    var invokeCount = 0;
    LazyDictionary<string, Task<string>>? lazyReverseMap = null;
    lazyReverseMap = new LazyDictionary<string, Task<string>>((dict, inStr) => {
      Assert.AreSame(lazyReverseMap, dict);
      invokeCount++;
      return Task.FromResult(inStr.Reverse());
    });

    Assert.AreEqual(0, lazyReverseMap.Count);
    Assert.AreEqual(0, invokeCount);

    Assert.AreEqual("esrever", await lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);

    // Reuses existing value
    Assert.AreEqual("esrever", await lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);
  }

  [Test]
  public async Task TestSettingValuesDirectly() {
    var lazyReverseMap = new LazyDictionary<string, Task<string>>(
        _ => throw new NotImplementedException());

    Assert.AreEqual(0, lazyReverseMap.Count);

    lazyReverseMap["reverse"] = Task.FromResult("esrever");
    Assert.AreEqual("esrever", await lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
  }

  [Test]
  public async Task TestClear() {
    var invokeCount = 0;
    var lazyReverseMap = new LazyDictionary<string, Task<string>>(inStr => {
      invokeCount++;
      return Task.FromResult(inStr.Reverse());
    });

    Assert.AreEqual(0, lazyReverseMap.Count);
    Assert.AreEqual(0, invokeCount);

    Assert.AreEqual("esrever", await lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);

    lazyReverseMap.Clear();
    Assert.AreEqual(0, lazyReverseMap.Count);
    Assert.AreEqual(1, invokeCount);

    Assert.AreEqual("esrever", await lazyReverseMap["reverse"]);
    Assert.AreEqual(1, lazyReverseMap.Count);
    Assert.AreEqual(2, invokeCount);
  }

  [Test]
  public async Task TestContainsKey() {
    var lazyReverseMap =
        new LazyDictionary<string, Task<string>>(inStr => Task.FromResult(inStr.Reverse()));

    Assert.AreEqual(false, lazyReverseMap.ContainsKey("reverse"));

    Assert.AreEqual("esrever", await lazyReverseMap["reverse"]);
    Assert.AreEqual(true, lazyReverseMap.ContainsKey("reverse"));

    lazyReverseMap.Clear();
    Assert.AreEqual(false, lazyReverseMap.ContainsKey("reverse"));
  }
}