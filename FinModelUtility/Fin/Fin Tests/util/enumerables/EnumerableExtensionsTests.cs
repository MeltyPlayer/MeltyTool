using System;
using System.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.util.enumerables;

public class EnumerableExtensionsTests {
  [Test]
  public void TestSeparatePairsWithNone()
    => CollectionAssert.AreEqual(
        Array.Empty<(int, int)>(),
        Array.Empty<int>().SeparatePairs());

  [Test]
  public void TestSeparatePairsWithMultiple()
    => CollectionAssert.AreEqual(
        new[] { (1, 2), (3, 4) },
        new[] { 1, 2, 3, 4 }.SeparatePairs());

  [Test]
  public void TestSeparatePairsWithLastPartial()
    => Assert.ThrowsException<InvalidOperationException>(
        () => new[] { 1, 2, 3, 4, 5 }.SeparatePairs().ToArray());


  [Test]
  public void TestSeparateTripletsWithNone()
    => CollectionAssert.AreEqual(
        Array.Empty<(int, int, int)>(),
        Array.Empty<int>().SeparateTriplets());

  [Test]
  public void TestSeparateTripletsWithMultiple()
    => CollectionAssert.AreEqual(
        new[] { (1, 2, 3), (4, 5, 6) },
        new[] { 1, 2, 3, 4, 5, 6 }.SeparateTriplets());

  [Test]
  public void TestSeparateTripletsWithLastPartial()
    => Assert.ThrowsException<InvalidOperationException>(
        () => new[] { 1, 2, 3, 4, 5 }.SeparateTriplets().ToArray());


  [Test]
  public void TestSeparateQuadrupletsWithNone()
    => CollectionAssert.AreEqual(
        Array.Empty<(int, int, int, int)>(),
        Array.Empty<int>().SeparateQuadruplets());

  [Test]
  public void TestSeparateQuadrupletsWithMultiple()
    => CollectionAssert.AreEqual(
        new[] { (1, 2, 3, 4), (5, 6, 7, 8) },
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.SeparateQuadruplets());

  [Test]
  public void TestSeparateQuadrupletsWithLastPartial()
    => Assert.ThrowsException<InvalidOperationException>(
        () => new[] { 1, 2, 3, 4, 5 }.SeparateQuadruplets().ToArray());
}