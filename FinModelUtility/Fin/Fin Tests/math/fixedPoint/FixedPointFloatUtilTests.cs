using NUnit.Framework;

namespace fin.math.fixedPoint;

internal class FixedPointFloatUtilTests {
  [Test]
  [TestCase((ushort) 0, ExpectedResult = 0)]
  [TestCase((ushort) 0x1000, ExpectedResult = 64f)]
  [TestCase((ushort) 0x12, ExpectedResult = .28125f)]
  [TestCase((ushort) 0x1234, ExpectedResult = 72.8125f)]
  [TestCase((ushort) 0xFFFF, ExpectedResult = -511.984375f)]
  public float TestRead16(ushort input)
    => FixedPointFloatUtil.Convert16(input, true, 9, 6);

  [Test]
  [TestCase((uint) 0, ExpectedResult = 0)]
  [TestCase((uint) 0x00001000, ExpectedResult = 1)]
  [TestCase((uint) 0x12345, ExpectedResult = 18.2043457f)]
  [TestCase((uint) 0x1234567, ExpectedResult = 4660.33789f)]
  [TestCase((uint) 0xFFFFFFF, ExpectedResult = 65535.999f)]
  [TestCase((uint) 0xFFFFFFFF, ExpectedResult = -524288)]
  public float TestRead32(uint input)
    => FixedPointFloatUtil.Convert32(input, true, 19, 12);
}