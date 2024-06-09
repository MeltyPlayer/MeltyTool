using System;
using System.Linq;

using NUnit.Framework;

namespace fin.util.enumerables {
  public class EnumerableExtensionsTests {
    [Test]
    public void TestSeparatePairsWithNone()
      => Assert.AreEqual(
          Array.Empty<(int, int)>(),
          Array.Empty<int>().SeparatePairs());

    [Test]
    public void TestSeparatePairsWithMultiple()
      => Assert.AreEqual(
          new[] { (1, 2), (3, 4) },
          new[] { 1, 2, 3, 4 }.SeparatePairs());

    [Test]
    public void TestSeparatePairsWithLastPartial()
      => Assert.Throws<InvalidOperationException>(
          () => new[] { 1, 2, 3, 4, 5 }.SeparatePairs().ToArray());


    [Test]
    public void TestSeparateTripletsWithNone()
      => Assert.AreEqual(
          Array.Empty<(int, int, int)>(),
          Array.Empty<int>().SeparateTriplets());

    [Test]
    public void TestSeparateTripletsWithMultiple()
      => Assert.AreEqual(
          new[] { (1, 2, 3), (4, 5, 6) },
          new[] { 1, 2, 3, 4, 5, 6 }.SeparateTriplets());

    [Test]
    public void TestSeparateTripletsWithLastPartial()
      => Assert.Throws<InvalidOperationException>(
          () => new[] { 1, 2, 3, 4, 5 }.SeparateTriplets().ToArray());


    [Test]
    public void TestSeparateQuadrupletsWithNone()
      => Assert.AreEqual(
          Array.Empty<(int, int, int, int)>(),
          Array.Empty<int>().SeparateQuadruplets());

    [Test]
    public void TestSeparateQuadrupletsWithMultiple()
      => Assert.AreEqual(
          new[] { (1, 2, 3, 4), (5, 6, 7, 8) },
          new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.SeparateQuadruplets());

    [Test]
    public void TestSeparateQuadrupletsWithLastPartial()
      => Assert.Throws<InvalidOperationException>(
          () => new[] { 1, 2, 3, 4, 5 }.SeparateQuadruplets().ToArray());
  }
}