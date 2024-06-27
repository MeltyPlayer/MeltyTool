using fin.math.floats;
using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.math.matrix.three;

public class Matrix3x2Tests {
  [Test]
  public void TestFloatArrayConstructor() {
    var values = new float[FinMatrix3x2.CELL_COUNT];
    for (var r = 0; r < FinMatrix3x2.ROW_COUNT; ++r) {
      for (var c = 0; c < FinMatrix3x2.COLUMN_COUNT; ++c) {
        values[FinMatrix3x2.COLUMN_COUNT * r + c] =
            FinMatrix3x2.COLUMN_COUNT * r + c;
      }
    }

    var mat = new FinMatrix3x2(values);

    for (var r = 0; r < FinMatrix3x2.ROW_COUNT; ++r) {
      for (var c = 0; c < FinMatrix3x2.COLUMN_COUNT; ++c) {
        Assert.AreEqual(FinMatrix3x2.COLUMN_COUNT * r + c, mat[r, c]);
      }
    }
  }

  [Test]
  public void TestDoubleArrayConstructor() {
    var values = new double[FinMatrix3x2.CELL_COUNT];
    for (var r = 0; r < FinMatrix3x2.ROW_COUNT; ++r) {
      for (var c = 0; c < FinMatrix3x2.COLUMN_COUNT; ++c) {
        values[FinMatrix3x2.COLUMN_COUNT * r + c] =
            FinMatrix3x2.COLUMN_COUNT * r + c;
      }
    }

    var mat = new FinMatrix3x2(values);

    for (var r = 0; r < FinMatrix3x2.ROW_COUNT; ++r) {
      for (var c = 0; c < FinMatrix3x2.COLUMN_COUNT; ++c) {
        Assert.AreEqual(FinMatrix3x2.COLUMN_COUNT * r + c, mat[r, c]);
      }
    }
  }

  [Test]
  public void TestCopyConstructor() {
    var values = new float[FinMatrix3x2.CELL_COUNT];
    for (var r = 0; r < FinMatrix3x2.ROW_COUNT; ++r) {
      for (var c = 0; c < FinMatrix3x2.COLUMN_COUNT; ++c) {
        values[FinMatrix3x2.COLUMN_COUNT * r + c] =
            FinMatrix3x2.COLUMN_COUNT * r + c;
      }
    }

    var first = new FinMatrix3x2(values);
    var second = new FinMatrix3x2(first);

    for (var r = 0; r < FinMatrix3x2.ROW_COUNT; ++r) {
      for (var c = 0; c < FinMatrix3x2.COLUMN_COUNT; ++c) {
        Assert.AreEqual(FinMatrix3x2.COLUMN_COUNT * r + c, second[r, c]);
      }
    }
  }

  [Test]
  public void TestMultiplyByMatrix() {
    var lhs = new FinMatrix3x2();
    lhs[0, 0] = 1;
    lhs[0, 1] = 2;

    lhs[1, 0] = 3;
    lhs[1, 1] = 4;
      
    lhs[2, 0] = 5;
    lhs[2, 1] = 6;

    var rhs = new FinMatrix3x2();
    rhs[0, 0] = 7;
    rhs[0, 1] = 8;
      
    rhs[1, 0] = 9;
    rhs[1, 1] = 10;
      
    rhs[2, 0] = 11;
    rhs[2, 1] = 12;

    var product = rhs.CloneAndMultiply(lhs);

    Assert.AreEqual(25, product[0, 0]);
    Assert.AreEqual(28, product[0, 1]);
 
    Assert.AreEqual(57, product[1, 0]);
    Assert.AreEqual(64, product[1, 1]);
      
    Assert.AreEqual(100, product[2, 0]);
    Assert.AreEqual(112, product[2, 1]);
  }

  [Test]
  public void TestMultiplyByScalar() {
    var inputMatrix = new FinMatrix3x2();
    inputMatrix[0, 0] = 1;
    inputMatrix[0, 1] = 2;

    inputMatrix[1, 0] = 3;
    inputMatrix[1, 1] = 5;
      
    inputMatrix[2, 0] = 6;
    inputMatrix[2, 1] = 7;

    inputMatrix.MultiplyInPlace(2);

    Assert.AreEqual(2, inputMatrix[0, 0]);
    Assert.AreEqual(4, inputMatrix[0, 1]);

    Assert.AreEqual(6, inputMatrix[1, 0]);
    Assert.AreEqual(10, inputMatrix[1, 1]);
      
    Assert.AreEqual(12, inputMatrix[2, 0]);
    Assert.AreEqual(14, inputMatrix[2, 1]);
  }

  [Test]
  public void TestInvert() {
    var inputMatrix = new FinMatrix3x2();
    inputMatrix[0, 0] = 2;
    inputMatrix[0, 1] = 5;

    inputMatrix[1, 0] = 0;
    inputMatrix[1, 1] = 1;
      
    inputMatrix[2, 0] = 4;
    inputMatrix[2, 1] = 2;

    var actualMatrix = inputMatrix.CloneAndInvert();

    var expectedMatrix = new FinMatrix3x2();
    expectedMatrix[0, 0] = .5f;
    expectedMatrix[0, 1] = -2.5f;

    expectedMatrix[1, 0] = 0;
    expectedMatrix[1, 1] = 1;
      
    expectedMatrix[2, 0] = -2;
    expectedMatrix[2, 1] = 8;

    for (var r = 0; r < FinMatrix3x2.ROW_COUNT; r++) {
      for (var c = 0; c < FinMatrix3x2.COLUMN_COUNT; c++) {
        Asserts.IsRoughly(expectedMatrix[r, c], actualMatrix[r, c]);
      }
    }
  }

  [Test]
  public void TestMultiplyByInverse() {
    var inputMatrix = new FinMatrix3x2();
    inputMatrix[0, 0] = 2;
    inputMatrix[0, 1] = 5;

    inputMatrix[1, 0] = 0;
    inputMatrix[1, 1] = 1;
      
    inputMatrix[2, 0] = 4;
    inputMatrix[2, 1] = 2;

    var inverseMatrix = inputMatrix.CloneAndInvert();

    var actualMatrix = inputMatrix.CloneAndMultiply(inverseMatrix);
    var expectedMatrix = FinMatrix3x2.IDENTITY;

    for (var r = 0; r < FinMatrix3x2.ROW_COUNT; r++) {
      for (var c = 0; c < FinMatrix3x2.COLUMN_COUNT; c++) {
        Asserts.IsRoughly(expectedMatrix[r, c], actualMatrix[r, c]);
      }
    }
  }

  [Test]
  public void TestCloseEquals() {
    var identityMatrix = FinMatrix3x2.IDENTITY;
    var closeToIdentityMatrix = this.GetCloseToIdentityMatrix_();
    Assert.AreEqual(identityMatrix, closeToIdentityMatrix);
  }

  [Test]
  public void TestCloseHashCode() {
    var identityMatrix = FinMatrix3x2.IDENTITY;
    var closeToIdentityMatrix = this.GetCloseToIdentityMatrix_();
    Assert.AreEqual(identityMatrix.GetHashCode(),
                    closeToIdentityMatrix.GetHashCode());
  }

  [Test]
  public void TestDifferentEquals() {
    var identityMatrix = FinMatrix3x2.IDENTITY;
    var differentFromIdentityMatrix = this.GetDifferentFromIdentityMatrix_();
    Assert.AreNotEqual(identityMatrix, differentFromIdentityMatrix);
  }

  [Test]
  public void TestDifferentHashCode() {
    var identityMatrix = FinMatrix3x2.IDENTITY;
    var differentFromIdentityMatrix = this.GetDifferentFromIdentityMatrix_();
    Assert.AreNotEqual(identityMatrix.GetHashCode(),
                       differentFromIdentityMatrix.GetHashCode());
  }


  private IReadOnlyFinMatrix3x2 GetCloseToIdentityMatrix_() {
    var closeToIdentityMatrix = new FinMatrix3x2().SetZero();

    var error = FloatsExtensions.ROUGHLY_EQUAL_ERROR * .1f;
    closeToIdentityMatrix[0, 0] = 1 + error;
    closeToIdentityMatrix[0, 1] = error;
      
    closeToIdentityMatrix[1, 0] = -error;
    closeToIdentityMatrix[1, 1] = 1 - error;
      
    closeToIdentityMatrix[2, 0] = error;
    closeToIdentityMatrix[2, 1] = error;

    return closeToIdentityMatrix;
  }

  private IReadOnlyFinMatrix3x2 GetDifferentFromIdentityMatrix_() {
    var closeToIdentityMatrix = new FinMatrix3x2().SetZero();

    var error = FloatsExtensions.ROUGHLY_EQUAL_ERROR * 10;
    closeToIdentityMatrix[0, 0] = 1 + error;
    closeToIdentityMatrix[0, 1] = error;

    closeToIdentityMatrix[1, 0] = -error;
    closeToIdentityMatrix[1, 1] = 1 - error;
      
    closeToIdentityMatrix[2, 0] = error;
    closeToIdentityMatrix[2, 1] = error;

    return closeToIdentityMatrix;
  }
}