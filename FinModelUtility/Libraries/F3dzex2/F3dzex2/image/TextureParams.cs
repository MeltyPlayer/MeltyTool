using System;

using f3dzex2.displaylist.opcodes;

using fin.model;
using fin.util.hash;

namespace f3dzex2.image;

public struct TextureParams : IEquatable<TextureParams> {
  public TextureParams() { }

  public ImageParams ImageParams { get; private set; } = new();

  public int Index { get; set; }

  public N64ColorFormat ColorFormat {
    get => this.ImageParams.ColorFormat;
    set {
      ImageParams imageParams = this.ImageParams;
      imageParams.ColorFormat = value;
      this.ImageParams = imageParams;
    }
  }

  public BitsPerTexel BitsPerTexel {
    get => this.ImageParams.BitsPerTexel;
    set {
      ImageParams imageParams = this.ImageParams;
      imageParams.BitsPerTexel = value;
      this.ImageParams = imageParams;
    }
  }

  public ushort Width {
    get => this.ImageParams.Width;
    set {
      ImageParams imageParams = this.ImageParams;
      imageParams.Width = value;
      this.ImageParams = imageParams;
    }
  }

  public ushort Height {
    get => this.ImageParams.Height;
    set {
      ImageParams imageParams = this.ImageParams;
      imageParams.Height = value;
      this.ImageParams = imageParams;
    }
  }

  public uint SegmentedAddress {
    get => this.ImageParams.SegmentedAddress;
    set {
      ImageParams imageParams = this.ImageParams;
      imageParams.SegmentedAddress = value;
      this.ImageParams = imageParams;
    }
  }

  public F3dWrapMode WrapModeT { get; set; } = F3dWrapMode.REPEAT;
  public F3dWrapMode WrapModeS { get; set; } = F3dWrapMode.REPEAT;

  public UvType UvType { get; set; } = UvType.STANDARD;

  public (ushort fullWidth, ushort uls, ushort ult)? LoadTileParams {
    get => this.ImageParams.LoadTileParams;
    set {
      ImageParams imageParams = this.ImageParams;
      imageParams.LoadTileParams = value;
      this.ImageParams = imageParams;
    }
  }

  public override int GetHashCode()
    => FluentHash.Start()
                 .With(this.Index)
                 .With(this.ImageParams)
                 .With(this.WrapModeT)
                 .With(this.WrapModeS)
                 .With(this.UvType)
                 .With(this.LoadTileParams ?? default);

  public bool Equals(TextureParams other)
    => this.Index.Equals(other.Index) &&
       this.ImageParams.Equals(other.ImageParams) &&
       this.WrapModeT == other.WrapModeT &&
       this.WrapModeS == other.WrapModeS &&
       this.UvType == other.UvType &&
       this.LoadTileParams.Equals(other.LoadTileParams);
}