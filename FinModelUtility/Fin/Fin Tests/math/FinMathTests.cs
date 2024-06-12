using NUnit.Framework;

namespace fin.math {
  public class FinMathTests {
    [Test]
    [TestCase(1, 0, 0, ExpectedResult = 0)]
    [TestCase(1, 0, 5, ExpectedResult = 1)]
    [TestCase(0, 0, 5, ExpectedResult = 0)]
    [TestCase(-1, 0, 5, ExpectedResult = 0)]
    [TestCase(5, 0, 5, ExpectedResult = 5)]
    [TestCase(6, 0, 5, ExpectedResult = 5)]
    public int TestClamp(int value, int min, int max)
      => value.Clamp(min, max);

    [Test]
    [TestCase(2, 0, 0, ExpectedResult = 0)]
    [TestCase(2, 1, 5, ExpectedResult = 2)]
    [TestCase(1, 1, 5, ExpectedResult = 1)]
    [TestCase(0, 1, 5, ExpectedResult = 4)]
    [TestCase(5, 1, 5, ExpectedResult = 5)]
    [TestCase(6, 1, 5, ExpectedResult = 2)]
    public int TestWrap(int value, int min, int max)
      => value.Wrap(min, max);

    [Test]
    [TestCase(2, 0, 0, 0)]
    [TestCase(2, 1, 5, 2)]
    [TestCase(1, 1, 5, 1)]
    [TestCase(.9f, 1, 5, 4.9f)]
    [TestCase(5, 1, 5, 5)]
    [TestCase(5, 1, 5, 5)]
    [TestCase(5.1f, 1, 5, 1.1f)]
    public void TestWrapFloat(float value, float min, float max, float expectedResult)
      => Assert.AreEqual(expectedResult, value.Wrap(min, max), .001f);
  }
}