using f3dzex2.displaylist.opcodes;


namespace f3dzex2.image;

public static class TmemExtensions {
  public static void SetImage(
      this ITmem tmem,
      uint imageSegmentedAddress,
      N64ColorFormat colorFormat,
      BitsPerTexel bitsPerTexel,
      ushort width,
      ushort height,
      F3dWrapMode wrapModeS,
      F3dWrapMode wrapModeT,
      uint tileDescriptorIndex = 0) {
    --width;
    --height;

    tmem.GsSpTexture(1,
                     1,
                     0,
                     TileDescriptorIndex.TX_LOADTILE,
                     TileDescriptorState.ENABLED);

    tmem.GsDpSetTextureImage(colorFormat,
                             bitsPerTexel,
                             width,
                             imageSegmentedAddress);
    tmem.GsDpSetTile(colorFormat,
                     bitsPerTexel,
                     default,
                     tileDescriptorIndex,
                     TileDescriptorIndex.TX_LOADTILE,
                     default,
                     wrapModeS,
                     default,
                     default,
                     wrapModeT,
                     default,
                     default);
    tmem.GsDpLoadBlock(0,
                       0,
                       TileDescriptorIndex.TX_LOADTILE,
                       (ushort) (width * height),
                       2048);
    tmem.GsDpSetTile(colorFormat,
                     bitsPerTexel,
                     default,
                     tileDescriptorIndex,
                     (TileDescriptorIndex) tileDescriptorIndex,
                     default,
                     wrapModeS,
                     default,
                     default,
                     wrapModeT,
                     default,
                     default);
    tmem.GsDpSetTileSize(0,
                         0,
                         (TileDescriptorIndex) tileDescriptorIndex,
                         width,
                         height);
  }
}