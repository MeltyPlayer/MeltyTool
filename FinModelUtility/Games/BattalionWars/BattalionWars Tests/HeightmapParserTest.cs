using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace modl.schema.terrain;

public class HeightmapParserTest {
  [Test]
  public void Test() {
    var chunkCountInAxis = 64;

    var expectedChunkX = 12;
    var expectedChunkY = 23;
    var expectedTileX = 0;
    var expectedTileY = 1;
    var expectedPointX = 1;
    var expectedPointY = 2;

    HeightmapParser.GetWorldPosition(
        chunkCountInAxis,
        chunkCountInAxis,
        expectedChunkX,
        expectedChunkY,
        expectedTileX,
        expectedTileY,
        expectedPointX,
        expectedPointY,
        out var worldX,
        out var worldY);

    HeightmapParser.GetIndices(
        worldX,
        worldY,
        chunkCountInAxis,
        chunkCountInAxis,
        out var actualChunkX,
        out var actualChunkY,
        out var actualTileX,
        out var actualTileY,
        out var actualPointX,
        out var actualPointY);

    Assert.AreEqual((expectedPointX, expectedPointY),
                    (actualPointX, actualPointY));
    Assert.AreEqual((expectedTileX, expectedTileY),
                    (actualTileX, actualTileY));
    Assert.AreEqual((expectedChunkX, expectedChunkY),
                    (actualChunkX, actualChunkY));
  }
}