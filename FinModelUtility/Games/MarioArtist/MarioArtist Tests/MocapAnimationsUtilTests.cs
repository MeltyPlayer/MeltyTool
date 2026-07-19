using fin.math.rotations;
using fin.util.asserts;

using marioartist.api;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace marioartist;

public sealed class MocapAnimationsUtilTests {
  [Test]
  [TestCase(0x3485, ExpectedResult = 0x348)]
  [TestCase(0xF6A0, ExpectedResult = 0xF6A)]
  public int TestTruncate(int x)
    => MocapAnimationsUtil.Truncate((short) x);

  [Test]
  [TestCase(0x3485, ExpectedResult = 73.8281326f)]
  [TestCase(0xF6A0, ExpectedResult = 346.816406f)]
  public float TestConvertShortToRadians(int x)
    => MocapAnimationsUtil.ConvertShortToRadians((short) x) * FinTrig.RAD_2_DEG;

  [Test]
  [TestCase(0x3485, 0.96043056f, 0.2785197f)]
  [TestCase(0xF6A0, -0.22807209f,  0.9736443f)]
  public void TestSinCos(int x, float expectedSin, float expectedCos) {
    var (actualSin, actualCos) = MathF.SinCos(MocapAnimationsUtil.ConvertShortToRadians((short) x));
    Asserts.IsRoughly(expectedSin, actualSin);
    Asserts.IsRoughly(expectedCos, actualCos);
  }
}