using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace modl.schema.anim;

public sealed class WeirdFloatMathTests {
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