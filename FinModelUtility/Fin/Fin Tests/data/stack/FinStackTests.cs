using NUnit.Framework;

namespace fin.data.stacks;

public class FinStackTests {
  [Test]
  public void TestInitWithSingle() {
    var stack = new FinStack<string>("foo");
    Assert.AreEqual("foo", stack.Top);
    Assert.AreEqual(1, stack.Count);
  }

  [Test]
  public void TestInitWithMultiple() {
    var stack = new FinStack<string>(new [] { "foo", "bar", });
    Assert.AreEqual(2, stack.Count);

    Assert.AreEqual("bar", stack.Pop());
    Assert.AreEqual(1, stack.Count);

    Assert.AreEqual("foo", stack.Pop());
    Assert.Zero(stack.Count);
  }

  [Test]
  public void TestPush() {
    var stack = new FinStack<string>();
    Assert.Zero(stack.Count);
      
    stack.Push("foo");
    Assert.AreEqual(1, stack.Count);
  }

  [Test]
  public void TestPushMultiple() {
    var stack = new FinStack<string>();
    Assert.Zero(stack.Count);

    stack.Push(new[] { "foo", "bar", "goo" });
    Assert.AreEqual(3, stack.Count);
  }

  [Test]
  public void TestPop() {
    var stack = new FinStack<string>();
    stack.Push("foo");
    stack.Push("bar");
    Assert.AreEqual(2, stack.Count);
 
    Assert.AreEqual("bar", stack.Pop());
    Assert.AreEqual(1, stack.Count);

    Assert.AreEqual("foo", stack.Pop());
    Assert.Zero(stack.Count);
  }

  [Test]
  public void TestTryPop() {
    var stack = new FinStack<string>();
    stack.Push("foo");
    stack.Push("bar");
    Assert.AreEqual(2, stack.Count);

    Assert.True(stack.TryPop(out var value0));
    Assert.AreEqual("bar", value0);
    Assert.AreEqual(1, stack.Count);

    Assert.True(stack.TryPop(out var value1));
    Assert.AreEqual("foo", value1);
    Assert.Zero(stack.Count);

    Assert.False(stack.TryPop(out _));
  }

  [Test]
  public void TestTop() {
    var stack = new FinStack<string>();
      
    stack.Push("foo");
    Assert.AreEqual("foo", stack.Top);

    stack.Push("bar");
    Assert.AreEqual("bar", stack.Top);

    stack.Top = "gar";
    Assert.AreEqual("gar", stack.Pop());

    stack.Top = "goo";
    Assert.AreEqual("goo", stack.Pop());
  }

  [Test]
  public void TestClear() {
    var stack = new FinStack<string>();

    stack.Push("foo");
    stack.Push("bar");

    stack.Clear();
    Assert.Zero(stack.Count);
    Assert.False(stack.TryPop(out _));
  }
}