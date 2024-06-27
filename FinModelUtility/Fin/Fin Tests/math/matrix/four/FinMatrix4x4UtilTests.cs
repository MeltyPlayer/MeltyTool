using System;
using System.Numerics;

using fin.math.rotations;
using fin.model;
using fin.util.asserts;

using NUnit.Framework;

namespace fin.math.matrix.four {
  public class FinMatrix4x4UtilTests {
    [Test]
    public void TestTranslation() {
      var expectedTranslation = new Vector3(2, 3, 4);

      var matrix = FinMatrix4x4Util.FromTranslation(
          expectedTranslation);

      matrix.CopyTranslationInto(out var actualTranslation);

      Assert.AreEqual(expectedTranslation, actualTranslation);
    }

    [Test]
    public void TestRotation() {
      var expectedRotation = QuaternionUtil.CreateZyx(1.2f, 2.3f, 3.4f);

      var matrix = FinMatrix4x4Util.FromRotation(
          expectedRotation);

      matrix.CopyRotationInto(out var actualRotation);

      Asserts.IsRoughly(expectedRotation.X, actualRotation.X);
      Asserts.IsRoughly(expectedRotation.Y, actualRotation.Y);
      Asserts.IsRoughly(expectedRotation.Z, actualRotation.Z);
      Asserts.IsRoughly(expectedRotation.W, actualRotation.W);
    }

    [Test]
    public void TestScale() {
      var expectedScale = new Vector3(3, 4, 5);

      var matrix = FinMatrix4x4Util.FromScale(
          expectedScale);

      matrix.CopyScaleInto(out var actualScale);

      Assert.AreEqual(expectedScale, actualScale);
    }


    [Test]
    public void TestTrs() {
      var expectedTranslation = new Vector3(2, 3, 4);
      var expectedRotation = QuaternionUtil.CreateZyx(1.2f, 2.3f, 3.4f);
      var expectedScale = new Vector3(3, 4, 5);

      var trs = FinMatrix4x4Util.FromTrs(
          expectedTranslation,
          expectedRotation,
          expectedScale);

      trs.CopyTranslationInto(out var actualTranslation);
      trs.CopyRotationInto(out var actualRotation);
      trs.CopyScaleInto(out var actualScale);

      Assert.AreEqual(expectedTranslation, actualTranslation);

      Assert.AreEqual(expectedScale, actualScale);

      Asserts.IsRoughly(expectedRotation.X, actualRotation.X);
      Asserts.IsRoughly(expectedRotation.Y, actualRotation.Y);
      Asserts.IsRoughly(expectedRotation.Z, actualRotation.Z);
      Asserts.IsRoughly(expectedRotation.W, actualRotation.W);
    }

    [Test]
    public void TestDecompose() {
      var expectedMatrix = new FinMatrix4x4(new[] {
          -0.690858f,
          0.000000f,
          0.722991f,
          0.000000f,
          0.000000f,
          1.000000f,
          0.000000f,
          0.000000f,
          -0.722991f,
          0.000000f,
          -0.690858f,
          0.000000f,
          -189.294998f,
          -2.000000f,
          -265.059998f,
          1.000000f
      }.AsSpan());

      expectedMatrix.Decompose(out var translation,
                               out var rotation,
                               out var scale);
      Assert.AreEqual(new Vector3(-189.294998f, -2.000000f, -265.059998f),
                      translation);

      var actualMatrix = FinMatrix4x4Util.FromTrs(translation, rotation, scale);
      Assert.AreEqual(expectedMatrix, actualMatrix);
    }
  }
}