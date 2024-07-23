using System.Numerics;

using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.math.matrix.three;

public class FinMatrix3x2UtilTests {
  [Test]
  public void TestTranslation() {
    var expectedTranslation = new Vector2(2, 3);

    var matrix = FinMatrix3x2Util.FromTranslation(expectedTranslation);

    matrix.CopyTranslationInto(out var actualTranslation);

    Assert.AreEqual(expectedTranslation, matrix.Impl.Translation);
    Assert.AreEqual(expectedTranslation, actualTranslation);
  }

  [Test]
  public void TestRotation() {
    var expectedRotation = 1.23f;

    var matrix = FinMatrix3x2Util.FromRotation(
        expectedRotation);

    matrix.CopyRotationInto(out var actualRotation);

    Assert.AreEqual(Vector2.Zero, matrix.Impl.Translation);
    Asserts.IsRoughly(expectedRotation, actualRotation);
  }

  [Test]
  public void TestScale() {
    var expectedScale = new Vector2(3, 4);

    var matrix = FinMatrix3x2Util.FromScale(expectedScale);

    matrix.CopyScaleInto(out var actualScale);

    Assert.AreEqual(Vector2.Zero, matrix.Impl.Translation);
    Assert.AreEqual(expectedScale, actualScale);
  }


  [Test]
  public void TestTrss() {
    var expectedTranslation = new Vector2(2, 3);
    var expectedRotation = 1.23f;
    var expectedScale = new Vector2(3, 4);
    var expectedSkew = 1.56f;

    var trs = FinMatrix3x2Util.FromTrss(
        expectedTranslation,
        expectedRotation,
        expectedScale,
        expectedSkew);

    trs.CopyTranslationInto(out var actualTranslation);
    trs.CopyRotationInto(out var actualRotation);
    trs.CopyScaleInto(out var actualScale);
    trs.CopySkewXRadiansInto(out var actualSkew);

    Assert.AreEqual(expectedTranslation, trs.Impl.Translation);
    Assert.AreEqual(expectedTranslation, actualTranslation);

    Asserts.IsRoughly(expectedRotation, actualRotation);

    Asserts.IsRoughly(expectedScale.X, actualScale.X);
    Asserts.IsRoughly(expectedScale.Y, actualScale.Y);

    Asserts.IsRoughly(expectedSkew, actualSkew);
  }
}