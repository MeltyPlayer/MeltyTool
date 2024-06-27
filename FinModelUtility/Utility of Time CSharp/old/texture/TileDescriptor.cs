namespace UoT;

/// <summary>
///   A given "tile descriptor" defined by F3DZEX2 display lists. This can be
///   thought of as the params for a texture. The display lists support
///   indexing 3 bits worth, so it seems that there can be at most 8 tile
///   descriptors active at once.
///
///   This *must* be a struct for now, as the parser expects for data to be
///   copied by value instead of by reference.
/// </summary>
public struct TileDescriptor {
  public bool Enabled;
  // TODO: Get rid of this field.
  public int ID;
  public int DXT;
  public int Height;
  public int Width;
  public int LoadWidth;
  public int LoadHeight;
  public double TextureHRatio;
  public double TextureWRatio;
  public uint TexBytes;
  public int Address;
  public int ImageBank;
  public ushort TmemOffset;
  public byte Palette;
  public int PaletteBank;
  public int Offset;
  public int PaletteOffset;
  // TODO: Delete this.
  public int JankFormat;
  public ColorFormat ColorFormat;
  public BitSize BitSize;
  public int CMS;
  public int CMT;
  public double S_Scale;
  public double T_Scale;
  public double ShiftS;
  public double ShiftT;
  public double TShiftS;
  public double TShiftT;
  public int OriginalMaskS;
  public int OriginalMaskT;
  public int MaskS;
  public int MaskT;
  public int LineSize;
  public int ULS;
  public int ULT;
  public int LRS;
  public int LRT;
  public uint OGLTexObj;
  public Color4UByte[] Palette32;

  public long Uuid {
    get {
        // These values impact how texture-clamping parameters are set in
        // OpenGl; they require unique texture instances.

        // The lower-right coords are each 3 bytes (12 bits) long.
        long lrs = this.LRS;
        long lrt = this.LRT;

        // The clamp flag values are 2 bits long.
        long cms = this.CMS;
        long cmt = this.CMT;

        long uuid = this.Address;
        uuid |= lrs << (32 + 4 + 12);
        uuid |= lrt << (32 + 4);
        uuid |= cms << (32 + 2);
        uuid |= cmt << 32;

        return uuid;
      }
  }
}