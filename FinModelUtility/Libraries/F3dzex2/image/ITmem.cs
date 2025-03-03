using f3dzex2.displaylist.opcodes;

using fin.model;

namespace f3dzex2.image;

/// <summary>
///   The N64 handles texture loading in a somewhat complicated way. Textures
///   are not referenced directly from memory, but rather through "tiles",
///   which are windows into a shared pool of memory.
///
///   Commands are used to load textures into this pool of memory, to set up
///   windows into this, etc. Windows don't have to exactly line up to where
///   a texture was previously loaded--this can be used to apply effects to
///   textures during the loading process!
///
///   https://fgfc.ddns.net/PerfectGold/Textures.htm
///   http://ultra64.ca/files/documentation/online-manuals/man/pro-man/pro13/index13.5.html
///   http://ultra64.ca/files/documentation/online-manuals/man/pro-man/pro13/13-09.html#:~:text=Internally%2C%20the%20RDP%20treats%20loading,the%20tile%20to%20be%20rendered.
/// </summary>
public interface ITmem {
  void GsDpLoadBlock(ushort uls,
                     ushort ult,
                     TileDescriptorIndex tileDescriptor,
                     ushort texels,
                     ushort deltaTPerScanline);

  void GsDpLoadTlut(TileDescriptorIndex tileDescriptor,
                    uint numColorsToLoad);

  void GsDpSetTile(N64ColorFormat colorFormat,
                   BitsPerTexel bitsPerTexel,
                   uint num64BitValuesPerRow,
                   uint offsetOfTextureInTmem,
                   TileDescriptorIndex tileDescriptor,
                   F3dWrapMode wrapModeS,
                   F3dWrapMode wrapModeT);

  void GsDpSetTileSize(ushort uls,
                       ushort ult,
                       TileDescriptorIndex tileDescriptor,
                       ushort width,
                       ushort height);

  void GsSpTexture(
      ushort scaleS,
      ushort scaleT,
      uint maxExtraMipmapLevels,
      TileDescriptorIndex tileDescriptor,
      TileDescriptorState tileDescriptorState);

  void GsDpSetTextureImage(N64ColorFormat colorFormat,
                           BitsPerTexel bitsPerTexel,
                           ushort width,
                           uint imageSegmentedAddress);

  MaterialParams GetMaterialParams();

  CullingMode CullingMode { set; }
}