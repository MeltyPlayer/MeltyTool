using f3dzex2.displaylist.opcodes;

using fin.model;
using fin.util.hash;

namespace f3dzex2.image;

public struct JankTileDescriptor : IReadOnlyTileDescriptor {
  public JankTileDescriptor() { }

  public MaterialParams MaterialParams { get; set; } = new();

  public TileDescriptorState State => TileDescriptorState.ENABLED;

  public override int GetHashCode()
    => FluentHash.Start().With(this.State).With(this.MaterialParams);

  public override bool Equals(object? obj) {
    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj is JankTileDescriptor otherTileDescriptor) {
      return this.State == otherTileDescriptor.State &&
             this.MaterialParams.Equals(otherTileDescriptor.MaterialParams);
    }

    return false;
  }
}

/// <summary>
///   This is a janky implementation of TMEM that picks and chooses which
///   params to use based on the tile descriptor. This approach sort of kind
///   of works for general cases, but isn't very accurate in general.
///
///   Generally, opcodes seem to be called in the following order:
///   1) set combine
///   2) set tile
///   3) set tile size
///   4) set timg
///   5) load block
///
///   References:
///   https://github.com/Emill/n64-fast3d-engine/blob/master/gfx_pc.c#L605
/// </summary>
public class JankTmem(IN64Hardware n64Hardware) : ITmem {
  private class SetTextureImageParams {
    public uint SegmentedAddress { get; set; }
    public N64ColorFormat ColorFormat { get; set; }
    public BitsPerTexel BitsPerTexel { get; set; }
    public ushort Width { get; set; }
    public uint TileNumber { get; set; }
  }

  private class LoadedTexture {
    public uint SegmentedAddress { get; set; }
    public uint SizeInBytes { get; set; }
  }

  private class TextureTile {
    public N64ColorFormat ColorFormat { get; set; }
    public BitsPerTexel BitsPerTexel { get; set; }
    public F3dWrapMode WrapModeS { get; set; }
    public F3dWrapMode WrapModeT { get; set; }
    public float Uls { get; set; }
    public float Ult { get; set; }
    public float Lrs { get; set; }
    public float Lrt { get; set; }
    public uint LineSizeBytes { get; set; }
  }

  private readonly SetTextureImageParams setTextureImageParams_ = new();
  private readonly TextureTile textureTile_ = new();

  private readonly LoadedTexture[] loadedTextures_ = [
      new LoadedTexture(), new LoadedTexture()
  ];

  private readonly TextureParams[] textureParams_ = new TextureParams[2];
  private readonly bool[] texturesChanged_ = [true, true];

  public void GsDpLoadBlock(float uls,
                            float ult,
                            TileDescriptorIndex tileDescriptor,
                            ushort texels,
                            ushort deltaTPerScanline) {
    if (tileDescriptor == TileDescriptorIndex.TX_LOADTILE) {
      // The lrs field rather seems to be number of pixels to load
      var sizeBytes
          = this.setTextureImageParams_.BitsPerTexel.GetByteCount(texels);
      this.loadedTextures_[this.setTextureImageParams_.TileNumber].SizeInBytes =
          sizeBytes;
      this.loadedTextures_[this.setTextureImageParams_.TileNumber]
          .SegmentedAddress =
          this.setTextureImageParams_.SegmentedAddress;
      this.texturesChanged_[this.setTextureImageParams_.TileNumber] = true;
    }
  }

  public void GsDpLoadTlut(TileDescriptorIndex tileDescriptor,
                           uint numColorsToLoad) {
    if (tileDescriptor == TileDescriptorIndex.TX_LOADTILE) {
      n64Hardware.Rdp.PaletteSegmentedAddress =
          this.setTextureImageParams_.SegmentedAddress;
    }
  }

  public void GsDpSetTile(N64ColorFormat colorFormat,
                          BitsPerTexel bitsPerTexel,
                          uint num64BitValuesPerRow,
                          uint offsetOfTextureInTmem,
                          TileDescriptorIndex tileDescriptor,
                          F3dWrapMode wrapModeS,
                          F3dWrapMode wrapModeT) {
    if (tileDescriptor == TileDescriptorIndex.TX_RENDERTILE) {
      this.textureTile_.ColorFormat = colorFormat;
      this.textureTile_.BitsPerTexel = bitsPerTexel;
      this.textureTile_.WrapModeT = wrapModeT;
      this.textureTile_.WrapModeS = wrapModeS;
      this.textureTile_.LineSizeBytes = num64BitValuesPerRow * 8;
      this.texturesChanged_[0] = true;
      this.texturesChanged_[1] = true;
    }

    if (tileDescriptor == TileDescriptorIndex.TX_LOADTILE) {
      this.setTextureImageParams_.TileNumber = offsetOfTextureInTmem / 256;
    }
  }

  public void GsDpSetTileSize(float uls,
                              float ult,
                              TileDescriptorIndex tileDescriptor,
                              float lrs,
                              float lrt) {
    if (tileDescriptor == TileDescriptorIndex.TX_RENDERTILE) {
      this.textureTile_.Uls = uls;
      this.textureTile_.Ult = ult;
      this.textureTile_.Lrs = lrs;
      this.textureTile_.Lrt = lrt;

      this.texturesChanged_[0] = true;
      this.texturesChanged_[1] = true;
    }
  }

  public void GsSpTexture(ushort scaleS,
                          ushort scaleT,
                          uint maxExtraMipmapLevels,
                          TileDescriptorIndex tileDescriptor,
                          TileDescriptorState tileDescriptorState) {
    n64Hardware.Rsp.TexScaleXShort = scaleS;
    n64Hardware.Rsp.TexScaleYShort = scaleT;
  }

  public void GsDpSetTextureImage(N64ColorFormat colorFormat,
                                  BitsPerTexel bitsPerTexel,
                                  ushort width,
                                  uint imageSegmentedAddress) {
    this.setTextureImageParams_.ColorFormat = colorFormat;
    this.setTextureImageParams_.SegmentedAddress = imageSegmentedAddress;
    this.setTextureImageParams_.Width = width;
    this.setTextureImageParams_.BitsPerTexel = bitsPerTexel;
  }

  public MaterialParams GetMaterialParams() {
    return new MaterialParams {
        TextureParams0 = this.GetOrCreateTextureParamsForTile_(0),
        TextureParams1 = this.GetOrCreateTextureParamsForTile_(1),
        CombinerCycleParams0 = n64Hardware.Rdp.CombinerCycleParams0,
        CombinerCycleParams1 = n64Hardware.Rdp.CombinerCycleParams1,
        CullingMode = this.cullingMode_
    };
  }

  private TextureParams GetOrCreateTextureParamsForTile_(int index) {
    if (!this.texturesChanged_[index]) {
      return this.textureParams_[index];
    }

    var loadedTexture = this.loadedTextures_[index];

    var textureParams = new TextureParams();
    textureParams.ColorFormat = this.textureTile_.ColorFormat;
    textureParams.BitsPerTexel = this.textureTile_.BitsPerTexel;

    textureParams.UvType =
        n64Hardware.Rsp.GeometryMode.GetUvType();
    textureParams.SegmentedAddress = loadedTexture.SegmentedAddress;
    textureParams.WrapModeS = this.textureTile_.WrapModeS;
    textureParams.WrapModeT = this.textureTile_.WrapModeT;

    /*textureParams.Width = (ushort) (textureTile_.LineSizeBytes >>
                                    textureParams.BitsPerTexel
                                                 .GetWordShift());
    textureParams.Height = textureTile_.LineSizeBytes == 0
        ? (ushort) 0
        : (ushort) (loadedTexture.SizeInBytes / textureTile_.LineSizeBytes);*/

    var uls = this.textureTile_.Uls;
    var ult = this.textureTile_.Ult;
    var lrs = this.textureTile_.Lrs;
    var lrt = this.textureTile_.Lrt;

    var width = lrs - uls + 1;
    var height = lrt - ult + 1;

    textureParams.Width = (ushort) width;
    textureParams.Height = (ushort) height;

    this.texturesChanged_[index] = false;
    return this.textureParams_[index] = textureParams;
  }

  private CullingMode cullingMode_;

  public CullingMode CullingMode {
    set {
      if (this.cullingMode_ != value) {
        this.cullingMode_ = value;
      }
    }
  }
}