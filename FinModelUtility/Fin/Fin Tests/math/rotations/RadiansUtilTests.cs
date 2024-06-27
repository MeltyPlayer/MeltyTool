using System;

using NUnit.Framework;

namespace fin.math.rotations;

[DefaultFloatingPointTolerance(.0001)]
public class RadiansUtilTests {
  private const float PI = MathF.PI;

  [Test]
  [TestCase(0, 0, ExpectedResult = 0)]
  [TestCase(PI, PI, ExpectedResult = 0)]
  [TestCase(2 * PI, 0, ExpectedResult = 0)]
  [TestCase(-2 * PI, 0, ExpectedResult = 0)]
  [TestCase(4 * PI, 0, ExpectedResult = 0)]
  [TestCase(PI, -PI, ExpectedResult = 0)]
  // Clockwise (positive)
  [TestCase(-PI / 4, PI / 4, ExpectedResult = PI / 2)]
  [TestCase(PI / 4, PI * 3 / 4, ExpectedResult = PI / 2)]
  [TestCase(PI * 3 / 4, -PI * 3 / 4, ExpectedResult = PI / 2)]
  [TestCase(PI * 7 / 4, PI / 4, ExpectedResult = PI / 2)]
  // Counterclockwise (Negative)
  [TestCase(PI / 4, -PI / 4, ExpectedResult = -PI / 2)]
  [TestCase(PI * 3 / 4, PI / 4, ExpectedResult = -PI / 2)]
  [TestCase(-PI * 3 / 4, PI * 3 / 4, ExpectedResult = -PI / 2)]
  [TestCase(PI / 4, PI * 7 / 4, ExpectedResult = -PI / 2)]
  public float TestCalculateRadiansTowardsExact(float from, float to)
    => RadiansUtil.CalculateRadiansTowards(from, to);

  [Test]
  // Clockwise (positive)
  [TestCase(0, PI, ExpectedResult = PI)]
  [TestCase(PI, 2 * PI, ExpectedResult = PI)]
  // Counterclockwise (Negative)
  [TestCase(PI, 0, ExpectedResult = -PI)]
  [TestCase(2 * PI, PI, ExpectedResult = -PI)]
  public float TestCalculateRadiansTowardsHalfRotation(float from, float to)
    => RadiansUtil.CalculateRadiansTowards(from, to);
}