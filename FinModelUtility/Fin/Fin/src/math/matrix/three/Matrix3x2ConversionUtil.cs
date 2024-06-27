using System.Numerics;

namespace fin.math.matrix.three;

public static class Matrix3x2ConversionUtil {
  public static void CopySystemIntoFin(
      Matrix3x2 other,
      IFinMatrix3x2 finMatrix) {
    finMatrix[0, 0] = other.M11;
    finMatrix[0, 1] = other.M21;
    finMatrix[0, 2] = other.M31;

    finMatrix[1, 0] = other.M12;
    finMatrix[1, 1] = other.M22;
    finMatrix[1, 2] = other.M32;
  }
}