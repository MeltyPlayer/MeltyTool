﻿using f3dzex2.displaylist.opcodes;

using fin.model;
using fin.util.hash;

namespace f3dzex2.image;

public struct TextureParams {
  public TextureParams() { }

  public ImageParams ImageParams { get; private set; } = new();

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

  public override int GetHashCode() => FluentHash.Start()
                                                 .With(this.ImageParams)
                                                 .With(this.WrapModeT)
                                                 .With(this.WrapModeS)
                                                 .With(this.UvType);

  public override bool Equals(object? other) {
      if (ReferenceEquals(this, other)) {
        return true;
      }

      if (other is TextureParams otherTextureParams) {
        return this.ImageParams.Equals(otherTextureParams.ImageParams) &&
               this.WrapModeT == otherTextureParams.WrapModeT &&
               this.WrapModeS == otherTextureParams.WrapModeS &&
               this.UvType == otherTextureParams.UvType;
      }

      return false;
    }
}