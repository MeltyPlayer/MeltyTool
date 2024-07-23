using System.Collections.Generic;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.util.lists;

public class ListBinarySearchExtensionsTests {
  public record A(int Value);

  public readonly struct AStaticComparer(int value)
      : IStaticAsymmetricComparer<A, int> {
    public int Value => value;
    public int Compare(A lhs, int rhs) => lhs.Value.CompareTo(rhs);
  }

  [Test]
  public void TestEasyBinarySearch() {
    var values = new List<A> { new(0), new(1), new(2) };

    var index = values.BinarySearch<A, int, AStaticComparer>(
        new AStaticComparer(1));

    Assert.AreEqual(1, index);
  }

  [Test]
  public void TestMissingBinarySearch() {
    var values = new List<A> { new(0), new(2) };

    var index = values.BinarySearch<A, int, AStaticComparer>(
        new AStaticComparer(1));

    Assert.AreEqual(~1, index);
  }

  [Test]
  public void TestTooLargeBinarySearch() {
    var values = new List<A> { new(0), new(2) };

    var index = values.BinarySearch<A, int, AStaticComparer>(
        new AStaticComparer(3));

    Assert.AreEqual(~2, index);
  }
}