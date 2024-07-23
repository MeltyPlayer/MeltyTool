using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.util.color;

public class ColorUtilTests {
  [Test]
  [TestCase(ushort.MinValue, 0, 0, 0)]
  [TestCase((ushort) 128, 0, 16, 0)]
  [TestCase((ushort) 255, 0, 28, 248)]
  [TestCase((ushort) 1000, 0, 124, 64)]
  [TestCase((ushort) 16383, 56, 252, 248)]
  [TestCase((ushort) 32767, 120, 252, 248)]
  [TestCase((ushort) 49151, 184, 252, 248)]
  [TestCase(ushort.MaxValue, 248, 252, 248)]
  public void TestSplitRgb565(ushort value,
                              byte expectedR,
                              byte expectedG,
                              byte expectedB) {
    ColorUtil.SplitRgb565(value,
                          out var actualR,
                          out var actualG,
                          out var actualB);
    Assert.AreEqual((expectedR, expectedG, expectedB),
                    (actualR, actualG, actualB));
  }
}