using System.Collections;
using System.Linq;

using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.data.queues;

public class FinTupleQueueTests {
  [Test]
  public void TestEnqueueSeparately() {
    var queue = new FinQueue<(string, string)>();
    Assert.AreEqual(0, queue.Count);

    queue.Enqueue(("foo1", "bar1"));
    Assert.AreEqual(1, queue.Count);

    queue.Enqueue(("foo2", "bar2"));
    Assert.AreEqual(2, queue.Count);

    Assert.AreEqual(("foo1", "bar1"), queue.Dequeue());
    Assert.AreEqual(1, queue.Count);
    Assert.AreEqual(("foo2", "bar2"), queue.Dequeue());
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void TestEnqueueParams() {
    var queue = new FinQueue<(string, string)>();
    Assert.AreEqual(0, queue.Count);

    queue.Enqueue(("foo1", "bar1"), ("foo2", "bar2"));
    Assert.AreEqual(2, queue.Count);

    queue.Enqueue(("foo3", "bar3"), ("foo4", "bar4"));
    Assert.AreEqual(4, queue.Count);

    Assert.AreEqual(("foo1", "bar1"), queue.Dequeue());
    Assert.AreEqual(3, queue.Count);
    Assert.AreEqual(("foo2", "bar2"), queue.Dequeue());
    Assert.AreEqual(2, queue.Count);
    Assert.AreEqual(("foo3", "bar3"), queue.Dequeue());
    Assert.AreEqual(1, queue.Count);
    Assert.AreEqual(("foo4", "bar4"), queue.Dequeue());
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void TestEnqueueEnumerables() {
    var queue = new FinQueue<(string, string)>();
    Assert.AreEqual(0, queue.Count);

    queue.Enqueue(new[] {("foo1", "bar1"), ("foo2", "bar2")});
    Assert.AreEqual(2, queue.Count);

    queue.Enqueue(new[] {("foo3", "bar3"), ("foo4", "bar4")});
    Assert.AreEqual(4, queue.Count);

    Assert.AreEqual(("foo1", "bar1"), queue.Dequeue());
    Assert.AreEqual(3, queue.Count);
    Assert.AreEqual(("foo2", "bar2"), queue.Dequeue());
    Assert.AreEqual(2, queue.Count);
    Assert.AreEqual(("foo3", "bar3"), queue.Dequeue());
    Assert.AreEqual(1, queue.Count);
    Assert.AreEqual(("foo4", "bar4"), queue.Dequeue());
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void TestConstructorParams() {
    var queue =
        new FinQueue<(string, string)>(("foo1", "bar1"), ("foo2", "bar2"));
    Assert.AreEqual(2, queue.Count);

    Assert.AreEqual(("foo1", "bar1"), queue.Dequeue());
    Assert.AreEqual(1, queue.Count);
    Assert.AreEqual(("foo2", "bar2"), queue.Dequeue());
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void TestConstructorEnumerable() {
    var queue =
        new FinQueue<(string, string)>(
            new[] {("foo1", "bar1"), ("foo2", "bar2")});
    Assert.AreEqual(2, queue.Count);

    Assert.AreEqual(("foo1", "bar1"), queue.Dequeue());
    Assert.AreEqual(1, queue.Count);
    Assert.AreEqual(("foo2", "bar2"), queue.Dequeue());
    Assert.AreEqual(0, queue.Count);
  }

  [Test]
  public void TestEnumeratorLinq() {
    var expectedValues =
        new[] {("foo1", "bar1"), ("foo2", "bar2"), ("foo3", "bar3")};
    var queue = new FinQueue<(string, string)>(expectedValues);

    var actualValues = queue.ToArray();
    Asserts.SequenceEqual(expectedValues, actualValues);
  }

  [Test]
  public void TestEnumeratorManually() {
    var queue =
        new FinQueue<(string, string)>(("foo1", "bar1"),
                                       ("foo2", "bar2"),
                                       ("foo3", "bar3"));

    var enumerator = ((IEnumerable) queue).GetEnumerator();

    Assert.AreEqual(true, enumerator.MoveNext());
    Assert.AreEqual(("foo1", "bar1"), enumerator.Current);

    Assert.AreEqual(true, enumerator.MoveNext());
    Assert.AreEqual(("foo2", "bar2"), enumerator.Current);

    Assert.AreEqual(true, enumerator.MoveNext());
    Assert.AreEqual(("foo3", "bar3"), enumerator.Current);

    Assert.AreEqual(false, enumerator.MoveNext());
  }

  [Test]
  public void TestClear() {
    var queue = new FinQueue<(string, string)>();
    Assert.AreEqual(0, queue.Count);

    queue.Enqueue(
        new[] {("foo1", "bar1"), ("foo2", "bar2"), ("foo3", "bar3")});
    Assert.AreEqual(3, queue.Count);

    queue.Clear();
    Assert.AreEqual(0, queue.Count);

    Assert.AreEqual(false, queue.TryPeek(out _));
    Assert.AreEqual(false, queue.TryDequeue(out _));
  }

  [Test]
  public void TestTryDequeue() {
    var queue = new FinQueue<(string, string)>();
    Assert.AreEqual(0, queue.Count);
    Assert.AreEqual(false, queue.TryDequeue(out _));

    var values = new[] { ("foo1", "bar1"), ("foo2", "bar2"), ("foo3", "bar3") };

    queue.Enqueue(values);
    Assert.AreEqual(3, queue.Count);

    foreach (var expectedValue in values) {
      Assert.AreEqual(true, queue.TryDequeue(out var actualValue));
      Assert.AreEqual(expectedValue, actualValue);
    }

    Assert.AreEqual(false, queue.TryDequeue(out _));
  }

  [Test]
  public void TestTryPeek() {
    var queue = new FinQueue<(string, string)>();
    Assert.AreEqual(0, queue.Count);
    Assert.AreEqual(false, queue.TryPeek(out _));

    var values = new[] {("foo1", "bar1"), ("foo2", "bar2"), ("foo3", "bar3") };

    queue.Enqueue(values);
    Assert.AreEqual(3, queue.Count);

    foreach (var expectedValue in values) {
      Assert.AreEqual(true, queue.TryPeek(out var actualValue));
      Assert.AreEqual(expectedValue, actualValue);

      queue.Dequeue();
    }

    Assert.AreEqual(false, queue.TryPeek(out _));
  }

  [Test]
  public void TestPeek() {
    var expectedValues =
        new[] { ("foo1", "bar1"), ("foo2", "bar2"), ("foo3", "bar3") };
    var queue = new FinQueue<(string, string)>(expectedValues);

    foreach (var expectedValue in expectedValues) {
      Assert.AreEqual(expectedValue, queue.Peek());

      queue.Dequeue();
    }
  }
}