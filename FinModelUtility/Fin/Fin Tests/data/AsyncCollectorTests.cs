using System.Threading.Tasks;

using fin.util.asserts;

using NUnit.Framework;

namespace fin.data;

public class AsyncCollectorTests {
  [Test]
  public async Task TestToArray() {
    var collector = new AsyncCollector<string>();

    var delay = new TaskCompletionSource();

    collector.Add("foo");
    collector.Add("bar");
    collector.Add(delay.Task.ContinueWith(_ => "awaited"));

    var toArray = collector.ToArray();
    collector.Clear();

    delay.SetResult();

    Expect.AreArraysEqual(["foo", "bar", "awaited"],
                          await toArray);
  }
}