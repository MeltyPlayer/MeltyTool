using System;
using System.Collections.Generic;
using System.Linq;

using f3dzex2.combiner;
using f3dzex2.displaylist.opcodes;

using fin.model;
using fin.util.asserts;

namespace f3dzex2.image;

public class TileState {
  public int cacheKey;
  public N64ColorFormat fmt;
  public BitsPerTexel siz;
  public uint line;
  public uint offsetOfTextureInTmem;
  public ushort palette;

  public F3dWrapMode wrapModeS;
  public ushort maskS;
  public ushort shiftS;

  public F3dWrapMode wrapModeT;
  public ushort maskT;
  public ushort shiftT;

  public float uls;
  public float ult;
  public float lrs;
  public float lrt;

  public ushort fullWidth;

  public void Set(
      N64ColorFormat fmt,
      BitsPerTexel siz,
      uint line,
      uint tmem,
      ushort palette,
      F3dWrapMode cms,
      ushort masks,
      ushort shifts,
      F3dWrapMode cmt,
      ushort maskt,
      ushort shiftT
  ) {
    this.fmt = fmt;
    this.siz = siz;
    this.line = line;
    this.offsetOfTextureInTmem = tmem;
    this.palette = palette;
    this.wrapModeS = cms;
    this.maskS = masks;
    this.shiftS = shifts;
    this.wrapModeT = cmt;
    this.maskT = maskt;
    this.shiftT = shiftT;
  }

  public void SetSize(float uls,
                      float ult,
                      float lrs,
                      float lrt,
                      ushort fullWidth) {
    this.uls = uls;
    this.ult = ult;
    this.lrs = lrs;
    this.lrt = lrt;
    this.fullWidth = fullWidth;
  }

  public void Copy(TileState o) {
    this.Set(o.fmt,
             o.siz,
             o.line,
             o.offsetOfTextureInTmem,
             o.palette,
             o.wrapModeS,
             o.maskS,
             o.shiftS,
             o.wrapModeT,
             o.maskT,
             o.shiftT);
    this.SetSize(o.uls, o.ult, o.lrs, o.lrt, o.fullWidth);
    this.cacheKey = o.cacheKey;
  }

  public float GetTileWidth() {
    var coordWidth = this.lrs - this.uls + 1;
    if (this.maskS != 0)
      return Math.Min(1 << this.maskS, coordWidth);
    else
      return coordWidth;
  }

  public float GetTileHeight() {
    var coordHeight = this.lrt - this.ult + 1;
    if (this.maskT != 0)
      return Math.Min(1 << this.maskT, coordHeight);
    else
      return coordHeight;
  }
}

public class TextureImageState {
  public N64ColorFormat colorFormat;
  public BitsPerTexel bitsPerTexel;
  public ushort width;
  public uint imageSegmentedAddress;

  public void Set(N64ColorFormat colorFormat,
                  BitsPerTexel bitsPerTexel,
                  ushort width,
                  uint imageSegmentedAddress) {
    this.colorFormat = colorFormat;
    this.bitsPerTexel = bitsPerTexel;
    this.width = width;
    this.imageSegmentedAddress = imageSegmentedAddress;
  }
}

public class TextureState {
  public float scaleS;
  public float scaleT;
  public uint maxExtraMipmapLevels;
  public TileDescriptorIndex tileDescriptor;
  public TileDescriptorState tileDescriptorState;

  public void Set(float scaleS,
                  float scaleT,
                  uint maxExtraMipmapLevels,
                  TileDescriptorIndex tileDescriptor,
                  TileDescriptorState tileDescriptorState) {
    this.scaleS = scaleS;
    this.scaleT = scaleT;
    this.maxExtraMipmapLevels = maxExtraMipmapLevels;
    this.tileDescriptor = tileDescriptor;
    this.tileDescriptorState = tileDescriptorState;
  }

  public void Copy(TextureState o)
    => this.Set(o.scaleS,
                o.scaleT,
                o.maxExtraMipmapLevels,
                o.tileDescriptor,
                o.tileDescriptorState);
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/40a191b5519d879396f1dc207d4553c775dc4d3c/src/PokemonSnap/f3dex2.ts
/// </summary>
public class NoclipTmem(IN64Hardware n64Hardware) : ITmem {
  private bool stateChanged_;
  private TextureState spTextureState_ = new();

  private readonly TextureImageState dpTextureImageState_ = new();

  private readonly TileState[] dpTileStates_ =
      Enumerable.Range(0, 8).Select(_ => new TileState()).ToArray();

  private readonly Dictionary<uint, uint> dpTmemTracker_ = new();

  public void GsDpLoadBlock(float uls,
                            float ult,
                            TileDescriptorIndex tileDescriptor,
                            ushort texels,
                            ushort dxt) {
    // First, verify that we're loading the whole texture.
    Asserts.True(uls == 0 && ult == 0);
    // Verify that we're loading into LOADTILE.
    Asserts.True(tileDescriptor == TileDescriptorIndex.TX_LOADTILE);

    var tile = this.dpTileStates_[(byte) tileDescriptor];
    // Compute the texture size from lrs/dxt. This is required for mipmapping to work correctly
    // in B-K due to hackery.
    var numWordsTotal = texels + 1;
    var numWordsInLine = (1 << 11) / dxt;
    var numPixelsInLine = (numWordsInLine * 8 * 8) / tile.siz.GetBitCount();
    tile.lrs = (numPixelsInLine - 1) << 2;
    tile.lrt = (((numWordsTotal / numWordsInLine) / 4) - 1) << 2;

    // Track the TMEM destination back to the originating DRAM address.
    this.dpTmemTracker_[tile.offsetOfTextureInTmem]
        = this.dpTextureImageState_.imageSegmentedAddress;
    this.stateChanged_ = true;
  }

  public void GsDpLoadTile(
      TileDescriptorIndex tileDescriptor,
      float uls,
      float ult,
      float lrs,
      float lrt) {
    // Verify that we're loading into LOADTILE.
    Asserts.True(tileDescriptor == TileDescriptorIndex.TX_LOADTILE);

    var tile = this.dpTileStates_[(byte) tileDescriptor];
    tile.SetSize(
        uls,
        ult,
        lrs,
        lrt,
        this.dpTextureImageState_.width);

    // Track the TMEM destination back to the originating DRAM address.
    this.dpTmemTracker_[tile.offsetOfTextureInTmem]
        = this.dpTextureImageState_.imageSegmentedAddress;
    this.stateChanged_ = true;
  }

  public void GsDpLoadTlut(TileDescriptorIndex tileDescriptor,
                           uint numColorsToLoad) {
    // Track the TMEM destination back to the originating DRAM address.
    var tmemDst = this.dpTileStates_[(byte) tileDescriptor]
                      .offsetOfTextureInTmem;
    this.dpTmemTracker_[tmemDst] =
        this.dpTextureImageState_.imageSegmentedAddress;

    // This is from JankTmem, may be wrong
    if (tileDescriptor == TileDescriptorIndex.TX_LOADTILE) {
      n64Hardware.Rdp.PaletteSegmentedAddress =
          this.dpTextureImageState_.imageSegmentedAddress;
    }
  }

  public void GsDpSetTile(N64ColorFormat colorFormat,
                          BitsPerTexel bitsPerTexel,
                          uint num64BitValuesPerRow,
                          uint offsetOfTextureInTmem,
                          TileDescriptorIndex tileDescriptor,
                          ushort palette,
                          F3dWrapMode wrapModeS,
                          ushort maskS,
                          ushort shiftS,
                          F3dWrapMode wrapModeT,
                          ushort maskT,
                          ushort shiftT) {
    this.dpTileStates_[(byte) tileDescriptor]
        .Set(colorFormat,
             bitsPerTexel,
             num64BitValuesPerRow,
             offsetOfTextureInTmem,
             palette,
             wrapModeS,
             maskS,
             shiftS,
             wrapModeT,
             maskT,
             shiftT);
    this.stateChanged_ = true;
  }

  public void GsDpSetTileSize(float uls,
                              float ult,
                              TileDescriptorIndex tileDescriptor,
                              float lrs,
                              float lrt)
    => this.dpTileStates_[(byte) tileDescriptor]
           .SetSize(uls,
                    ult,
                    lrs,
                    lrt,
                    this.dpTextureImageState_.width);

  public void GsSpTexture(ushort scaleS,
                          ushort scaleT,
                          uint maxExtraMipmapLevels,
                          TileDescriptorIndex tileDescriptor,
                          TileDescriptorState tileDescriptorState) {
    // This is the texture we're using to rasterize triangles going forward.
    this.spTextureState_.Set((1f * scaleS) / 0x10000,
                             (1f * scaleT) / 0x10000,
                             maxExtraMipmapLevels,
                             tileDescriptor,
                             tileDescriptorState);
    this.stateChanged_ = true;
  }

  public void GsDpSetTextureImage(N64ColorFormat colorFormat,
                                  BitsPerTexel bitsPerTexel,
                                  ushort width,
                                  uint imageSegmentedAddress)
    => this.dpTextureImageState_.Set(
        colorFormat,
        bitsPerTexel,
        width,
        imageSegmentedAddress);

  public MaterialParams GetMaterialParams() {
    return new MaterialParams {
        TextureParams0 = this.GetOrCreateTextureParamsForTile_(0),
        TextureParams1 = this.GetOrCreateTextureParamsForTile_(1),
        CombinerCycleParams0 = n64Hardware.Rdp.CombinerCycleParams0,
        CombinerCycleParams1 = n64Hardware.Rdp.CycleType == CycleType.TWO_CYCLE
            ? n64Hardware.Rdp.CombinerCycleParams1
            : null,
        CullingMode = this.cullingMode_
    };
  }

  private TextureParams? GetOrCreateTextureParamsForTile_(int index) {
    if (this.spTextureState_.tileDescriptorState ==
        TileDescriptorState.DISABLED) {
      return null;
    }

    var rdp = n64Hardware.Rdp;
    if (index == 1 && rdp.CycleType != CycleType.TWO_CYCLE) {
      return null;
    }

    var tile = this.dpTileStates_[index];

    var textureParams = new TextureParams();

    textureParams.Index = index;

    textureParams.ColorFormat = tile.fmt;
    textureParams.BitsPerTexel = tile.siz;

    textureParams.UvType = n64Hardware.Rsp.GeometryMode.GetUvType();
    textureParams.SegmentedAddress
        = this.dpTmemTracker_[tile.offsetOfTextureInTmem];

    textureParams.WrapModeS = tile.wrapModeS;
    textureParams.WrapModeT = tile.wrapModeT;

    textureParams.Width = (ushort) tile.GetTileWidth();
    textureParams.Height = (ushort) tile.GetTileHeight();

    if (tile.uls != 0 || tile.ult != 0) {
      textureParams.LoadTileParams = (
          (ushort) (tile.fullWidth + 1), (ushort) tile.uls, (ushort) tile.ult);
    }

    return textureParams;
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