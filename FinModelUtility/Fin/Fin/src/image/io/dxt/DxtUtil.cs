using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace fin.image.io.dxt;

using DxtSubTileTuple = (int imageX, int imageY, int byteOffset);

public static class DxtUtil {
  public static IEnumerable<DxtSubTileTuple> ListSubtilesWith4Loops(
      int width,
      int height,
      int subTileCountInAxis,
      int subTileSizeInAxis) {
    var tileSize = subTileCountInAxis * subTileSizeInAxis;

    var tileXCount = (int) Math.Ceiling(1f * width / tileSize);
    var tileYCount = (int) Math.Ceiling(1f * height / tileSize);

    var subblockIndex = 0;
    for (var tileY = 0; tileY < tileYCount; ++tileY) {
      for (var tileX = 0; tileX < tileXCount; ++tileX) {
        for (var j = 0; j < subTileCountInAxis; ++j) {
          for (var i = 0; i < subTileCountInAxis; ++i) {
            var imageX = tileX * tileSize + i * subTileSizeInAxis;
            var imageY = tileY * tileSize + j * subTileSizeInAxis;

            var byteOffset = (subblockIndex++) * 8;

            yield return (imageX, imageY, byteOffset);
          }
        }
      }
    }
  }

  public static IEnumerable<DxtSubTileTuple> ListSubtilesWith1Loop(
      int width,
      int height,
      int subTileCountInAxis,
      int subTileSizeInAxis) {
    var tileSize = subTileCountInAxis * subTileSizeInAxis;

    var tileXCount = (int) Math.Ceiling(1f * width / tileSize);
    var tileYCount = (int) Math.Ceiling(1f * height / tileSize);

    var subTilesPerTile = subTileCountInAxis * subTileCountInAxis;
    var subTileCount = tileXCount *
                       tileYCount *
                       subTilesPerTile;

    for (var subTileI = 0; subTileI < subTileCount; ++subTileI) {
      yield return GetSubTileTuple(
          tileXCount,
          tileSize,
          subTileCountInAxis,
          subTileSizeInAxis,
          subTilesPerTile,
          subTileI);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static DxtSubTileTuple GetSubTileTuple(
      int tileXCount,
      int tileSize,
      int subTileCountInAxis,
      int subTileSizeInAxis,
      int subTilesPerTile,
      int subTileI) {
    var tileI = subTileI / subTilesPerTile;
    var tileX = tileI % tileXCount;
    var tileY = tileI / tileXCount;

    var internalSubTileI = subTileI % subTilesPerTile;
    var subTileX = internalSubTileI % subTileCountInAxis;
    var subTileY = internalSubTileI / subTileCountInAxis;

    var imageX = tileX * tileSize + subTileX * subTileSizeInAxis;
    var imageY = tileY * tileSize + subTileY * subTileSizeInAxis;

    var byteOffset = subTileI * 8;

    return (imageX, imageY, byteOffset);
  }
}