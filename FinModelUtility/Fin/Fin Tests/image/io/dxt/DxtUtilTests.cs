using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace fin.image.io.dxt;

internal class DxtUtilTests {
  [Test]
  [TestCase(64, 64, 2, 4)]
  [TestCase(64, 32, 2, 4)]
  [TestCase(32, 64, 2, 4)]
  [TestCase(50, 50, 2, 4)]
  [TestCase(32, 32, 2, 4)]
  public void TestListsSameSubTilesFrom4LoopsAnd1Loop(
      int width,
      int height,
      int subTileCountInAxis,
      int subTileSizeInAxis) {
    var subTilesFrom4Loops
        = DxtUtil.ListSubtilesWith4Loops(width,
                                         height,
                                         subTileCountInAxis,
                                         subTileSizeInAxis);
    var subTilesFrom1Loop
        = DxtUtil.ListSubtilesWith1Loop(width,
                                        height,
                                        subTileCountInAxis,
                                        subTileSizeInAxis);

    CollectionAssert.AreEquivalent(subTilesFrom4Loops, subTilesFrom1Loop);
  }
}