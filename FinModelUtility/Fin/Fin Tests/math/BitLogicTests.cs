using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.math;

public class BitLogicTests {
  [Test]
  public void ExtractFromRight() {
    Assert.AreEqual((uint) 0b1111,
                    BitLogic.ExtractFromRight(0b00001111, 0, 4));
    Assert.AreEqual((uint) 0b1111,
                    BitLogic.ExtractFromRight(0b11110000, 4, 4));
    Assert.AreEqual((uint) 0b1011, BitLogic.ExtractFromRight(0b101100, 2, 4));
  }

  [Test]
  [TestCase(1, 0, ExpectedResult = true)]
  [TestCase(2, 0, ExpectedResult = false)]
  [TestCase(2, 1, ExpectedResult = true)]
  public bool TestGetBitInt(int value, int bit) => value.GetBit(bit);

  [Test]
  [TestCase(0, ExpectedResult = 0)]
  [TestCase(1, ExpectedResult = 0b1)]
  [TestCase(2, ExpectedResult = 0b11)]
  [TestCase(3, ExpectedResult = 0b111)]
  [TestCase(4, ExpectedResult = 0b1111)]
  public int TestMask(int numBits) => (int) BitLogic.GetMask(numBits);

  [Test]
  [DefaultFloatingPointTolerance(.00000001)]
  [TestCase((ushort) 0, ExpectedResult = 0)]
  [TestCase((ushort) 0x1, ExpectedResult = 0.0000152587890625)]
  [TestCase((ushort) 0x0080, ExpectedResult = 0.001953125)]
  [TestCase((ushort) 0x8000, ExpectedResult = .5)]
  [TestCase((ushort) 0xFFFF, ExpectedResult = 0.9999847412109375)]
  public double TestConvertBinaryFractionToFloat(ushort inputBinaryFraction)
    => BitLogic.ConvertBinaryFractionToDouble(inputBinaryFraction);
}