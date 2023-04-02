using asserts;

using fin.math.matrix;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;


namespace fin.math {
  public class MatrixTests {
    [Test]
    public void TestFloatArrayConstructor() {
      var values = new float[16];
      for (var r = 0; r < 4; ++r) {
        for (var c = 0; c < 4; ++c) {
          values[4 * r + c] = 4 * r + c;
        }
      }

      var mat = new FinMatrix4x4(values);

      for (var r = 0; r < 4; ++r) {
        for (var c = 0; c < 4; ++c) {
          Assert.AreEqual(4 * r + c, mat[r, c]);
        }
      }
    }

    [Test]
    public void TestDoubleArrayConstructor() {
      var values = new double[16];
      for (var r = 0; r < 4; ++r) {
        for (var c = 0; c < 4; ++c) {
          values[4 * r + c] = 4 * r + c;
        }
      }

      var mat = new FinMatrix4x4(values);

      for (var r = 0; r < 4; ++r) {
        for (var c = 0; c < 4; ++c) {
          Assert.AreEqual(4 * r + c, mat[r, c]);
        }
      }
    }

    [Test]
    public void TestCopyConstructor() {
      var values = new float[16];
      for (var r = 0; r < 4; ++r) {
        for (var c = 0; c < 4; ++c) {
          values[4 * r + c] = 4 * r + c;
        }
      }

      var first = new FinMatrix4x4(values);
      var second = new FinMatrix4x4(first);

      for (var r = 0; r < 4; ++r) {
        for (var c = 0; c < 4; ++c) {
          Assert.AreEqual(4 * r + c, second[r, c]);
        }
      }
    }

    [Test]
    public void TestMultiplyByMatrix() {
      var lhs = new FinMatrix4x4();
      lhs[0, 0] = 1;
      lhs[0, 1] = 2;
      lhs[0, 2] = 3;
      lhs[0, 3] = 4;
      lhs[1, 0] = 5;
      lhs[1, 1] = 6;
      lhs[1, 2] = 7;
      lhs[1, 3] = 8;
      lhs[2, 0] = 9;
      lhs[2, 1] = 10;
      lhs[2, 2] = 11;
      lhs[2, 3] = 12;
      lhs[3, 0] = 13;
      lhs[3, 1] = 14;
      lhs[3, 2] = 15;
      lhs[3, 3] = 16;

      var rhs = new FinMatrix4x4();
      rhs[0, 0] = 4;
      rhs[0, 1] = 5;
      rhs[0, 2] = 6;
      rhs[0, 3] = 2;
      rhs[1, 0] = 5;
      rhs[1, 1] = 8;
      rhs[1, 2] = 9;
      rhs[1, 3] = 3;
      rhs[2, 0] = 1;
      rhs[2, 1] = 3;
      rhs[2, 2] = 4;
      rhs[2, 3] = 6;
      rhs[3, 0] = 7;
      rhs[3, 1] = 8;
      rhs[3, 2] = 9;
      rhs[3, 3] = 5;

      var product = rhs.CloneAndMultiply(lhs);

      Assert.AreEqual(45, product[0, 0]);
      Assert.AreEqual(62, product[0, 1]);
      Assert.AreEqual(72, product[0, 2]);
      Assert.AreEqual(46, product[0, 3]);
      Assert.AreEqual(113, product[1, 0]);
      Assert.AreEqual(158, product[1, 1]);
      Assert.AreEqual(184, product[1, 2]);
      Assert.AreEqual(110, product[1, 3]);
      Assert.AreEqual(181, product[2, 0]);
      Assert.AreEqual(254, product[2, 1]);
      Assert.AreEqual(296, product[2, 2]);
      Assert.AreEqual(174, product[2, 3]);
      Assert.AreEqual(249, product[3, 0]);
      Assert.AreEqual(350, product[3, 1]);
      Assert.AreEqual(408, product[3, 2]);
      Assert.AreEqual(238, product[3, 3]);
    }

    [Test]
    public void TestMultiplyByScalar() {
      var inputMatrix = new FinMatrix4x4();
      inputMatrix[0, 0] = 1;
      inputMatrix[0, 1] = 2;
      inputMatrix[0, 2] = 3;
      inputMatrix[0, 3] = 4;
      inputMatrix[1, 0] = 5;
      inputMatrix[1, 1] = 6;
      inputMatrix[1, 2] = 7;
      inputMatrix[1, 3] = 8;
      inputMatrix[2, 0] = 9;
      inputMatrix[2, 1] = 10;
      inputMatrix[2, 2] = 11;
      inputMatrix[2, 3] = 12;
      inputMatrix[3, 0] = 13;
      inputMatrix[3, 1] = 14;
      inputMatrix[3, 2] = 15;
      inputMatrix[3, 3] = 16;

      inputMatrix.MultiplyInPlace(2);

      Assert.AreEqual(2, inputMatrix[0, 0]);
      Assert.AreEqual(4, inputMatrix[0, 1]);
      Assert.AreEqual(6, inputMatrix[0, 2]);
      Assert.AreEqual(8, inputMatrix[0, 3]);
      Assert.AreEqual(10, inputMatrix[1, 0]);
      Assert.AreEqual(12, inputMatrix[1, 1]);
      Assert.AreEqual(14, inputMatrix[1, 2]);
      Assert.AreEqual(16, inputMatrix[1, 3]);
      Assert.AreEqual(18, inputMatrix[2, 0]);
      Assert.AreEqual(20, inputMatrix[2, 1]);
      Assert.AreEqual(22, inputMatrix[2, 2]);
      Assert.AreEqual(24, inputMatrix[2, 3]);
      Assert.AreEqual(26, inputMatrix[3, 0]);
      Assert.AreEqual(28, inputMatrix[3, 1]);
      Assert.AreEqual(30, inputMatrix[3, 2]);
      Assert.AreEqual(32, inputMatrix[3, 3]);
    }

    [Test]
    public void TestInvert() {
      var inputMatrix = new FinMatrix4x4();
      inputMatrix[0, 0] = 2;
      inputMatrix[0, 1] = 5;
      inputMatrix[0, 2] = 0;
      inputMatrix[0, 3] = 8;
      inputMatrix[1, 0] = 1;
      inputMatrix[1, 1] = 4;
      inputMatrix[1, 2] = 2;
      inputMatrix[1, 3] = 6;
      inputMatrix[2, 0] = 7;
      inputMatrix[2, 1] = 8;
      inputMatrix[2, 2] = 9;
      inputMatrix[2, 3] = 3;
      inputMatrix[3, 0] = 1;
      inputMatrix[3, 1] = 5;
      inputMatrix[3, 2] = 7;
      inputMatrix[3, 3] = 8;

      var actualMatrix = inputMatrix.CloneAndInvert();

      var expectedMatrix = new FinMatrix4x4();
      expectedMatrix[0, 0] = 172f / 179;
      expectedMatrix[0, 1] = -343f / 179;
      expectedMatrix[0, 2] = 14f / 179;
      expectedMatrix[0, 3] = 80f / 179;
      expectedMatrix[1, 0] = -185f / 179;
      expectedMatrix[1, 1] = 422f / 179;
      expectedMatrix[1, 2] = 12f / 179;
      expectedMatrix[1, 3] = -136f / 179;
      expectedMatrix[2, 0] = -1f / 179;
      expectedMatrix[2, 1] = -49f / 179;
      expectedMatrix[2, 2] = 2f / 179;
      expectedMatrix[2, 3] = 37f / 179;
      expectedMatrix[3, 0] = 95f / 179;
      expectedMatrix[3, 1] = -178f / 179;
      expectedMatrix[3, 2] = -11f / 179;
      expectedMatrix[3, 3] = 65f / 179;

      for (var r = 0; r < 4; r++) {
        for (var c = 0; c < 4; c++) {
          Assert.AreEqual(expectedMatrix[r, c], actualMatrix[r, c], .0001f);
        }
      }
    }

    [Test]
    public void TestMultiplyByInverse() {
      var inputMatrix = new FinMatrix4x4();
      inputMatrix[0, 0] = 2;
      inputMatrix[0, 1] = 5;
      inputMatrix[0, 2] = 0;
      inputMatrix[0, 3] = 8;
      inputMatrix[1, 0] = 1;
      inputMatrix[1, 1] = 4;
      inputMatrix[1, 2] = 2;
      inputMatrix[1, 3] = 6;
      inputMatrix[2, 0] = 7;
      inputMatrix[2, 1] = 8;
      inputMatrix[2, 2] = 9;
      inputMatrix[2, 3] = 3;
      inputMatrix[3, 0] = 1;
      inputMatrix[3, 1] = 5;
      inputMatrix[3, 2] = 7;
      inputMatrix[3, 3] = 8;

      var inverseMatrix = inputMatrix.CloneAndInvert();

      var actualMatrix = inputMatrix.CloneAndMultiply(inverseMatrix);
      var expectedMatrix = FinMatrix4x4.IDENTITY;

      for (var r = 0; r < 4; r++) {
        for (var c = 0; c < 4; c++) {
          Assert.AreEqual(expectedMatrix[r, c], actualMatrix[r, c], .0001f);
        }
      }
    }

    [Test]
    public void TestCloseEquals() {
      var identityMatrix = FinMatrix4x4.IDENTITY;
      var closeToIdentityMatrix = this.GetCloseToIdentityMatrix_();
      Asserts.True(identityMatrix.Equals(closeToIdentityMatrix));
    }

    [Test]
    public void TestCloseHashCode() {
      var identityMatrix = FinMatrix4x4.IDENTITY;
      var closeToIdentityMatrix = this.GetCloseToIdentityMatrix_();
      Asserts.Equal(identityMatrix.GetHashCode(),
                    closeToIdentityMatrix.GetHashCode());
    }

    [Test]
    public void TestDifferentEquals() {
      var identityMatrix = FinMatrix4x4.IDENTITY;
      var differentFromIdentityMatrix = this.GetDifferentFromIdentityMatrix_();
      Asserts.False(identityMatrix.Equals(differentFromIdentityMatrix));
    }

    [Test]
    public void TestDifferentHashCode() {
      var identityMatrix = FinMatrix4x4.IDENTITY;
      var differentFromIdentityMatrix = this.GetDifferentFromIdentityMatrix_();
      Assert.AreNotEqual(identityMatrix.GetHashCode(),
                         differentFromIdentityMatrix.GetHashCode());
    }


    private IReadOnlyFinMatrix4x4 GetCloseToIdentityMatrix_() {
      var closeToIdentityMatrix = new FinMatrix4x4().SetZero();

      var error = .00001f;
      closeToIdentityMatrix[0, 0] = 1 + error;
      closeToIdentityMatrix[0, 1] = error;
      closeToIdentityMatrix[0, 2] = -error;
      closeToIdentityMatrix[0, 3] = error;
      closeToIdentityMatrix[1, 0] = -error;
      closeToIdentityMatrix[1, 1] = 1 - error;
      closeToIdentityMatrix[1, 2] = error;
      closeToIdentityMatrix[1, 3] = -error;
      closeToIdentityMatrix[2, 0] = error;
      closeToIdentityMatrix[2, 1] = -error;
      closeToIdentityMatrix[2, 2] = 1 + error;
      closeToIdentityMatrix[2, 3] = error;
      closeToIdentityMatrix[3, 0] = -error;
      closeToIdentityMatrix[3, 1] = error;
      closeToIdentityMatrix[3, 2] = -error;
      closeToIdentityMatrix[3, 3] = 1 - error;

      return closeToIdentityMatrix;
    }

    private IReadOnlyFinMatrix4x4 GetDifferentFromIdentityMatrix_() {
      var closeToIdentityMatrix = new FinMatrix4x4().SetZero();

      var error = .001f;
      closeToIdentityMatrix[0, 0] = 1 + error;
      closeToIdentityMatrix[0, 1] = error;
      closeToIdentityMatrix[0, 2] = -error;
      closeToIdentityMatrix[0, 3] = error;
      closeToIdentityMatrix[1, 0] = -error;
      closeToIdentityMatrix[1, 1] = 1 - error;
      closeToIdentityMatrix[1, 2] = error;
      closeToIdentityMatrix[1, 3] = -error;
      closeToIdentityMatrix[2, 0] = error;
      closeToIdentityMatrix[2, 1] = -error;
      closeToIdentityMatrix[2, 2] = 1 + error;
      closeToIdentityMatrix[2, 3] = error;
      closeToIdentityMatrix[3, 0] = -error;
      closeToIdentityMatrix[3, 1] = error;
      closeToIdentityMatrix[3, 2] = -error;
      closeToIdentityMatrix[3, 3] = 1 - error;

      return closeToIdentityMatrix;
    }
  }
}