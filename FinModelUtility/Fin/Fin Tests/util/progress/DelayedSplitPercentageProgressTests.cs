using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using CollectionAssert = NUnit.Framework.CollectionAssert;

namespace fin.util.progress;

public sealed class DelayedSplitPercentageProgressTests {
  [Test]
  public void TestDefaultProgress() {
    var progress = new DelayedSplitPercentageProgress(3);
    Assert.AreEqual(0, progress.Progress);
  }

  [Test]
  public void TestEachProgresses() {
    var progress = new DelayedSplitPercentageProgress(3);

    var sub0 = progress.Add();
    var sub1 = progress.Add();
    var sub2 = progress.Add();

    var actualProgressChanges = new List<float>();
    var completeCallCount = 0;
    progress.OnProgressChanged += (sender, f) => actualProgressChanges.Add(f);
    progress.OnComplete += (sender, f) => ++completeCallCount;

    sub0.ReportProgress(.25f);
    CollectionAssert.AreEqual(new [] {.25f / 3}, actualProgressChanges);
    Assert.AreEqual(0, completeCallCount);

    sub1.ReportProgress(.5f);
    CollectionAssert.AreEqual(new [] {.25f / 3, (.25f + .5f) / 3}, actualProgressChanges);
    Assert.AreEqual(0, completeCallCount);

    sub2.ReportProgress(.75f);
    CollectionAssert.AreEqual(new [] {.25f / 3, (.25f + .5f) / 3, (.25f + .5f + .75f) / 3}, actualProgressChanges);
    Assert.AreEqual(0, completeCallCount);
  }

  [Test]
  public void TestEachCompletes() {
    var progress = new DelayedSplitPercentageProgress(3);

    var sub0 = progress.Add();
    var sub1 = progress.Add();
    var sub2 = progress.Add();

    var actualProgressChanges = new List<float>();
    var completeCallCount = 0;
    progress.OnProgressChanged += (sender, f) => actualProgressChanges.Add(f);
    progress.OnComplete += (sender, f) => ++completeCallCount;

    sub0.ReportCompletion();
    CollectionAssert.AreEqual(new [] {1f / 3}, actualProgressChanges);
    Assert.AreEqual(0, completeCallCount);

    sub1.ReportCompletion();
    CollectionAssert.AreEqual(new [] {1f / 3, 2f / 3}, actualProgressChanges);
    Assert.AreEqual(0, completeCallCount);

    sub2.ReportCompletion();
    CollectionAssert.AreEqual(new [] {1f / 3, 2f / 3}, actualProgressChanges);
    Assert.AreEqual(1, completeCallCount);
  }
}