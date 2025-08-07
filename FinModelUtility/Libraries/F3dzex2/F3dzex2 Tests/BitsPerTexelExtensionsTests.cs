using NUnit.Framework;

namespace f3dzex2.image;

public class BitsPerTexelExtensionsTests {
  [Test]
  [TestCase(BitsPerTexel._4BPT, (uint) 2, ExpectedResult = (uint) 1)]
  [TestCase(BitsPerTexel._8BPT, (uint) 1, ExpectedResult = (uint) 1)]
  [TestCase(BitsPerTexel._16BPT, (uint) 1, ExpectedResult = (uint) 2)]
  public uint TestGetByteCount(BitsPerTexel bitsPerTexel, uint texelCount)
    => bitsPerTexel.GetByteCount(texelCount);
}