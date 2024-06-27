using System;

using fin.util.asserts;

using NUnit.Framework;

namespace fin.math.rotations;

public class FinTrigTests {
  [Test]
  [TestCase(-.5f * MathF.PI)]
  [TestCase(0)]
  [TestCase(.5f * MathF.PI)]
  [TestCase(MathF.PI)]
  [TestCase(1.5f * MathF.PI)]
  [TestCase(2 * MathF.PI)]
  [TestCase(2.5f * MathF.PI)]
  public void TestSin(float radians)
    => Asserts.IsRoughly(MathF.Sin(radians), FinTrig.Sin(radians));

  [Test]
  [TestCase(-.5f * MathF.PI)]
  [TestCase(0)]
  [TestCase(.5f * MathF.PI)]
  [TestCase(MathF.PI)]
  [TestCase(1.5f * MathF.PI)]
  [TestCase(2 * MathF.PI)]
  [TestCase(2.5f * MathF.PI)]
  public void TestCos(float radians)
    => Asserts.IsRoughly(MathF.Cos(radians), FinTrig.Cos(radians));

  [Test]
  [TestCase(-1)]
  [TestCase(-.5f)]
  [TestCase(0)]
  [TestCase(.5f)]
  [TestCase(1)]
  public void TestAsin(float radians)
    => Asserts.IsRoughly(MathF.Asin(radians), FinTrig.Asin(radians));

  [Test]
  [TestCase(-1)]
  [TestCase(-.5f)]
  [TestCase(0)]
  [TestCase(.5f)]
  [TestCase(1)]
  public void TestAcos(float radians)
    => Asserts.IsRoughly(MathF.Acos(radians), FinTrig.Acos(radians));

  [Test]
  [TestCase(-1, -1)]
  [TestCase(-1, 1)]
  [TestCase(1, -1)]
  [TestCase(1, 1)]
  public void TestAtan2(float x, float y)
    => Asserts.IsRoughly(MathF.Atan2(y, x), FinTrig.Atan2(y, x));
}