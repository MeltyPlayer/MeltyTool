using System;
using System.Linq;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.util.enumerables;

public class EnumerableExtensionsTests {
  [Test]
  public void TestSeparatePairsWithNone()
    => Assert.AreEqual(
        Array.Empty<(int, int)>(),
        Array.Empty<int>().SeparatePairs());

  [Test]
  public void TestSeparatePairsWithMultiple()
    => Assert.AreEqual(
        [(1, 2), (3, 4)],
        new[] { 1, 2, 3, 4 }.SeparatePairs());

  [Test]
  public void TestSeparatePairsWithLastPartial()
    => Assert.ThrowsException<InvalidOperationException>(
        () => new[] { 1, 2, 3, 4, 5 }.SeparatePairs().ToArray());


  [Test]
  public void TestSeparateTripletsWithNone()
    => Assert.AreEqual(
        Array.Empty<(int, int, int)>(),
        Array.Empty<int>().SeparateTriplets());

  [Test]
  public void TestSeparateTripletsWithMultiple()
    => Assert.AreEqual(
        [(1, 2, 3), (4, 5, 6)],
        new[] { 1, 2, 3, 4, 5, 6 }.SeparateTriplets());

  [Test]
  public void TestSeparateTripletsWithLastPartial()
    => Assert.ThrowsException<InvalidOperationException>(
        () => new[] { 1, 2, 3, 4, 5 }.SeparateTriplets().ToArray());


  [Test]
  public void TestSeparateQuadrupletsWithNone()
    => Assert.AreEqual(
        Array.Empty<(int, int, int, int)>(),
        Array.Empty<int>().SeparateQuadruplets());

  [Test]
  public void TestSeparateQuadrupletsWithMultiple()
    => Assert.AreEqual(
        [(1, 2, 3, 4), (5, 6, 7, 8)],
        new[] { 1, 2, 3, 4, 5, 6, 7, 8 }.SeparateQuadruplets());

  [Test]
  public void TestSeparateQuadrupletsWithLastPartial()
    => Assert.ThrowsException<InvalidOperationException>(
        () => new[] { 1, 2, 3, 4, 5 }.SeparateQuadruplets().ToArray());
}