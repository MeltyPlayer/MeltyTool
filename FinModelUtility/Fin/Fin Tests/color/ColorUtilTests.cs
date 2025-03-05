using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.color;

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
  public void TestRgb565(ushort value,
                         byte expectedR,
                         byte expectedG,
                         byte expectedB) {
    ColorUtil.SplitRgb565(value,
                          out var actualR,
                          out var actualG,
                          out var actualB);
    Assert.AreEqual((expectedR, expectedG, expectedB),
                    (actualR, actualG, actualB));

    var color = ColorUtil.ParseRgb565(value);
    Assert.AreEqual((color.Rb, color.Gb, color.Bb),
                    (actualR, actualG, actualB));
  }

  [Test]
  [TestCase(ushort.MinValue, 0, 0, 0, 0)]
  [TestCase((ushort) 128, 0, 136, 0, 0)]
  [TestCase((ushort) 255, 0, 255, 255, 0)]
  [TestCase((ushort) 1000, 51, 238, 136, 0)]
  [TestCase((ushort) 16383, 255, 255, 255, 95)]
  [TestCase((ushort) 32767, 255, 255, 255, 223)]
  [TestCase((ushort) 49151, 119, 247, 247, 255)]
  [TestCase(ushort.MaxValue, 247, 247, 247, 255)]
  public void TestRgb5A3(ushort value,
                         byte expectedR,
                         byte expectedG,
                         byte expectedB,
                         byte expectedA) {
    ColorUtil.SplitRgb5A3(value,
                          out var actualR,
                          out var actualG,
                          out var actualB,
                          out var actualA);
    Assert.AreEqual((expectedR, expectedG, expectedB, expectedA),
                    (actualR, actualG, actualB, actualA));

    var color = ColorUtil.ParseRgb5A3(value);
    Assert.AreEqual((color.Rb, color.Gb, color.Bb, color.Ab),
                    (actualR, actualG, actualB, actualA));
  }

  [Test]
  [TestCase(ushort.MinValue, 0, 0, 0, 0)]
  [TestCase((ushort) 128, 0, 31, 0, 0)]
  [TestCase((ushort) 255, 0, 55, 247, 0)]
  [TestCase((ushort) 1000, 0, 247, 63, 0)]
  [TestCase((ushort) 16383, 119, 247, 247, 0)]
  [TestCase((ushort) 32767, 247, 247, 247, 0)]
  [TestCase((ushort) 49151, 119, 247, 247, 255)]
  [TestCase(ushort.MaxValue, 247, 247, 247, 255)]
  public void TestRgb5A1(ushort value,
                         byte expectedR,
                         byte expectedG,
                         byte expectedB,
                         byte expectedA) {
    ColorUtil.SplitRgb5A1(value,
                          out var actualR,
                          out var actualG,
                          out var actualB,
                          out var actualA);
    Assert.AreEqual((expectedR, expectedG, expectedB, expectedA),
                    (actualR, actualG, actualB, actualA));

    var color = ColorUtil.ParseRgb5A1(value);
    Assert.AreEqual((color.Rb, color.Gb, color.Bb, color.Ab),
                    (actualR, actualG, actualB, actualA));
  }
}