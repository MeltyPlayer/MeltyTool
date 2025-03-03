using System.Numerics;

using fin.math;

namespace visceral.api;

public static class PackedVectorUtil {
  public static Vector3
      ExtractNormalFromUInt32(uint value) {
    var bitsPerAxis = 10;
    var divisor = 512f;
    Vector3 vec = default;
    for (var i = 0; i < 3; ++i) {
      var axisValue = value.ExtractFromRight(bitsPerAxis * i, bitsPerAxis);
      vec[i] = GetSignedValue(axisValue, bitsPerAxis);
    }

    return vec / divisor;
  }

  public static Vector4 ExtractTangentFromUInt32(uint value)
    => new(ExtractNormalFromUInt32(value), 1);

  public static int GetSignedValue(uint x, int bitsPerAxis) {
    var isSigned = x.GetBit(bitsPerAxis - 1);
    var mask = BitLogic.GetMask(bitsPerAxis - 1);

    var value = (int) (x & mask);

    if (!isSigned) {
      return value;
    }

    return (int) (-Math.Pow(2, bitsPerAxis - 1) + value);
  }
}