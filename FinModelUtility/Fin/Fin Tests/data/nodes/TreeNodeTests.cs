using System.Collections.Generic;
using System.Linq;

using fin.data.queues;
using fin.util.asserts;

using NUnit.Framework;

namespace fin.data.nodes;

public class TreeNodeTests {
  [Test]
  public void TestIterating() {
    var nodeRoot = new TreeNode<string> { Value = "root" };

    var nodeFoo = new TreeNode<string> { Value = "foo" };
    nodeRoot.AddChild(nodeFoo);
    var nodeBar = new TreeNode<string> { Value = "bar" };
    nodeRoot.AddChild(nodeBar);

    var node123 = new TreeNode<string> { Value = "123" };
    nodeFoo.AddChild(node123);

    var nodeAbc = new TreeNode<string> { Value = "abc" };
    nodeBar.AddChild(nodeAbc);

    var actualValues = new List<string>();
    var finQueue = new FinQueue<ITreeNode<string>>(nodeRoot);
    while (finQueue.TryDequeue(out var node)) {
      actualValues.Add(node.Value);
      finQueue.Enqueue(node.ChildNodes);
    }

    Asserts.SequenceEqual<IEnumerable<string>>(
        actualValues,
        new[] { "root", "foo", "bar", "123", "abc" });
  }

  [Test]
  public void TestAncestors() {
    var nodeRoot = new TreeNode<string> { Value = "root" };

    var nodeFoo = new TreeNode<string> { Value = "foo" };
    nodeRoot.AddChild(nodeFoo);

    var node123 = new TreeNode<string> { Value = "123" };
    nodeFoo.AddChild(node123);

    var actualValues = new List<string>();
    var finQueue = new FinQueue<ITreeNode<string>>(node123);
    while (finQueue.TryDequeue(out var node)) {
      actualValues.Add(node.Value);

      if (node.Parent != null) {
        finQueue.Enqueue(node.Parent);
      }
    }

    Asserts.SequenceEqual<IEnumerable<string>>(
        actualValues,
        new[] { "123", "foo", "root" });
  }

  [Test]
  public void TestReplacingParent() {
    var nodeFoo = new TreeNode<string> { Value = "foo" };
    var nodeBar = new TreeNode<string> { Value = "foo" };

    Assert.AreEqual(0, nodeFoo.ChildNodes.Count());
    Assert.AreEqual(0, nodeBar.ChildNodes.Count());

    var nodeChild = new TreeNode<string> { Value = "child" };
    Assert.Null(nodeChild.Parent);

    nodeChild.Parent = nodeFoo;
    Assert.AreEqual(nodeFoo, nodeChild.Parent);
    Asserts.SequenceEqual(nodeFoo.ChildNodes, new[] { nodeChild });
    Assert.AreEqual(0, nodeBar.ChildNodes.Count());

    nodeBar.AddChild(nodeChild);
    Assert.AreEqual(nodeBar, nodeChild.Parent);
    Asserts.SequenceEqual(nodeBar.ChildNodes, new[] { nodeChild });
    Assert.AreEqual(0, nodeFoo.ChildNodes.Count());

    nodeChild.Parent = nodeFoo;
    Assert.AreEqual(nodeFoo, nodeChild.Parent);
    Asserts.SequenceEqual(nodeFoo.ChildNodes, new[] { nodeChild });
    Assert.AreEqual(0, nodeBar.ChildNodes.Count());
  }
}