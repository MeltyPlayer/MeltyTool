using NUnit.Framework.Legacy;

namespace uni.ui.avalonia.common.progress;

public class AsyncProgressTests {
  [Test]
  public void TestFromResult() {
    var value = "hello";
    var ap = AsyncProgress.FromResult(value);
    AssertComplete_(ap, value);
  }

  [Test]
  public void TestFromTaskResult() {
    var value = "hello";
    var ap = AsyncProgress.FromTask(Task.FromResult(value));
    AssertComplete_(ap, value);
  }

  [Test]
  public void TestManual() {
    var ap = new AsyncProgress();
    AssertIncomplete_(ap);

    var value = "hello";
    ap.ReportCompletion(value);

    AssertComplete_(ap, value);
  }


  [Test]
  // TODO: Fix this flakiness...

  /*public void TestFromTaskDelayed() {
    var task = new TaskCompletionSource<string>();
    var ap = AsyncProgress.FromTask(task.Task);
    AssertIncomplete_(ap);

    var value = "hello";
    task.TrySetResult(value);

    ClassicAssert.AreEqual(value, task.Task.Result);

    AssertComplete_(ap, value);
  }*/

  private static void AssertIncomplete_(AsyncProgress ap) {
    ClassicAssert.False(ap.IsComplete);
    ClassicAssert.Null(ap.Value);
  }

  private static void AssertComplete_(AsyncProgress ap, object value) {
    ClassicAssert.True(ap.IsComplete);
    ClassicAssert.AreEqual(value, ap.Value);
  }
}