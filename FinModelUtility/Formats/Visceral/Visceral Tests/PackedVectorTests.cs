namespace visceral.api;

public class PackedVectorTests {
  [Test]
  [TestCase(0b1000000010, ExpectedResult = -510)]
  [TestCase(0b1101010110, ExpectedResult = -170)]
  [TestCase(0b1110100111, ExpectedResult = -89)]
  [TestCase(0b1111010111, ExpectedResult = -41)]
  [TestCase(0b1111100001, ExpectedResult = -31)]
  [TestCase(0b0000011111, ExpectedResult = 31)]
  [TestCase(0b0000101010, ExpectedResult = 42)]
  [TestCase(0b0001011001, ExpectedResult = 89)]
  [TestCase(0b0111110110, ExpectedResult = 502)]
  [TestCase(0b0111111111, ExpectedResult = 511)]
  public int TestGetSignedValue(int value)
    =>PackedVectorUtil.GetSignedValue((uint) value, 10);
}