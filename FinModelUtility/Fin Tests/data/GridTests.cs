using System;

using NUnit.Framework;


namespace fin.data {
  public class GridTests {
    [Test]
    public void TestDefaultValue() {
      var impl = new Grid<string>(3, 3, "foobar");

      Assert.AreEqual(3, impl.Width);
      Assert.AreEqual(3, impl.Height);

      for (var y = 0; y < impl.Height; ++y) {
        for (var x = 0; x < impl.Width; ++x) {
          Assert.AreEqual("foobar", impl[x, y]);
        }
      }
    }

    [Test]
    public void TestDefaultHandler() {
      var index = 0;
      var impl = new Grid<int>(3, 3, () => index++);

      Assert.AreEqual(3, impl.Width);
      Assert.AreEqual(3, impl.Height);

      for (var y = 0; y < impl.Height; ++y) {
        for (var x = 0; x < impl.Width; ++x) {
          Assert.AreEqual(y * impl.Width + x, impl[x, y]);
        }
      }
    }

    [Test]
    public void TestSetValues() {
      var impl = new Grid<string>(3, 3);

      Assert.AreEqual(3, impl.Width);
      Assert.AreEqual(3, impl.Height);

      for (var y = 0; y < impl.Height; ++y) {
        for (var x = 0; x < impl.Width; ++x) {
          impl[x, y] = $"({x}, {y})";
        }
      }

      for (var y = 0; y < impl.Height; ++y) {
        for (var x = 0; x < impl.Width; ++x) {
          Assert.AreEqual($"({x}, {y})", impl[x, y]);
        }
      }
    }

    [Test]
    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(4, 0)]
    [TestCase(0, 4)]
    public void TestFailsOutside(int x, int y) {
      var impl = new Grid<string>(3, 3);
      try {
        impl[x, y] = "value";
        Assert.Fail("Expected to throw exception");
      } catch (Exception e) {
        Assert.AreEqual(
            $"Expected ({x}, {y}) to be a valid index in grid of size (3, 3).",
            e.Message);
      }
    }
  }
}