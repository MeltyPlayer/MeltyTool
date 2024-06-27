namespace modl.schema.anim;

public class WeirdFloatMathTests {
  [Test]
  [TestCase(3100000000, -0.00009453f)]
  [TestCase(3150000000, -0.005895f)]
  [TestCase(3200000000, -0.367431f)]
  [TestCase(3250000000, -22.883056f)]
  [TestCase(3300000000, -1424.03125f)]
  [TestCase((uint) 0x0229C4AB, WeirdFloatMath.C_ZERO)]
  [TestCase((uint) 0x38000000, WeirdFloatMath.C_3_05175_EN5)]
  [TestCase((uint) 0x38800000, WeirdFloatMath.C_6_10351_EN5)]
  [TestCase((uint) 0x3F000000, WeirdFloatMath.C_HALF)]
  [TestCase((uint) 0x40400000, WeirdFloatMath.C_3)]
  [TestCase((uint) 0x46800000, WeirdFloatMath.C_16384)]
  public void TestInterpretAsSingle(uint input, float expectedOutput)
    => Assert.AreEqual(expectedOutput,
                       WeirdFloatMath.InterpretAsSingle(input),
                       .000001f);

  [Test]
  [TestCase(13746744073709551615, -2.6518952414567028E-06d)]
  [TestCase(13796744073709551615, -0.0058304183539235046d)]
  [TestCase(13846744073709551615, -12.758538758847861d)]
  [TestCase(13896744073709551615, -27804.427732706066d)]
  [TestCase(13946744073709551615, -60373745.84277343d)]
  [TestCase((ulong) 0x4330000000000000, WeirdFloatMath.C_4503599627370496)]
  public void TestInterpretAsDouble(ulong input, double expectedOutput)
    => Assert.AreEqual(expectedOutput,
                       WeirdFloatMath.InterpretAsDouble(input),
                       .0000001);
}