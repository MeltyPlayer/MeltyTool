using System;

namespace fin.math.fixedPoint;

public static class FixedPointFloatUtil {
  public static float Convert16(ushort x,
                                bool hasSign,
                                int integerBits,
                                int fractionBits) {
    var sign = 1;
    if (hasSign) {
      // sign extend
      var signMask = (uint) (1 << (integerBits + fractionBits));
      if ((x & signMask) != 0) {
        sign = -1;
        x |= (ushort) ~(signMask - 1);
      }
    }

    var fractionPart = x.ExtractFromRight(0, fractionBits);
    var integerPart = x.ExtractFromRight(fractionBits, integerBits);

    return sign * (integerPart + fractionPart * MathF.Pow(0.5f, fractionBits));
  }

  public static float Convert32(uint x,
                                bool hasSign,
                                int integerBits,
                                int fractionBits) {
    var sign = 1;
    if (hasSign) {
      // sign extend
      var signMask = (uint) (1 << (integerBits + fractionBits));
      if ((x & signMask) != 0) {
        sign = -1;
        x |= ~(signMask - 1);
      }
    }

    var fractionPart = x.ExtractFromRight(0, fractionBits);
    var integerPart = x.ExtractFromRight(fractionBits, integerBits);

    return sign * (integerPart + fractionPart * MathF.Pow(0.5f, fractionBits));
  }
}