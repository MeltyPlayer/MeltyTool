using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.image;
using fin.model;
using fin.util.hash;

using gx;

using jsystem.schema.jutility.bti;

namespace jsystem.exporter;

public class BmdGxTexture : IGxTexture {
  public BmdGxTexture(
      string name,
      Bti header,
      IList<(string, Bti)>? pathsAndBtis = null) {
    this.Name = name;
    this.DefaultHeader = header;

    // TODO: This doesn't feel right, where can we get the actual name?
    if (pathsAndBtis != null && name.Contains("_dummy")) {
      var prefix = name.Substring(0, name.IndexOf("_dummy")).ToLower();

      var matchingPathAndBtis = pathsAndBtis
          .SkipWhile(pathAndBti
                         => !new FileInfo(pathAndBti.Item1)
                             .Name.ToLower()
                             .StartsWith(prefix));

      if (matchingPathAndBtis.Count() > 0) {
        var matchingPathAndBti = matchingPathAndBtis.First();

        this.Name = new FileInfo(matchingPathAndBti.Item1).Name;
        var bti = matchingPathAndBti.Item2;

        this.OverrideHeader = bti;
      }
    }

    this.MipmapImages = this.Header.ToMipmapImages();

    this.ColorType = BmdGxTexture.GetColorType_(this.Header.Format);
  }

  public string Name { get; }
  public override string ToString() => this.Name;

  public IReadOnlyImage[] MipmapImages { get; }
  public Bti Header => OverrideHeader ?? DefaultHeader;
  private Bti DefaultHeader { get; }
  private Bti? OverrideHeader { get; }

  public GxWrapMode WrapModeS => this.Header.WrapS;
  public GxWrapMode WrapModeT => this.Header.WrapT;
  public GX_MIN_TEXTURE_FILTER MinTextureFilter => this.Header.MinFilter;
  public GX_MAG_TEXTURE_FILTER MagTextureFilter => this.Header.MagFilter;

  public ColorType ColorType { get; }

  private static ColorType GetColorType_(GxTextureFormat textureFormat) {
    switch (textureFormat) {
      case GxTextureFormat.I4:
      case GxTextureFormat.I8:
      case GxTextureFormat.A4_I4:
      case GxTextureFormat.A8_I8:
        return ColorType.INTENSITY;

      case GxTextureFormat.R5_G6_B5:
      case GxTextureFormat.A3_RGB5:
      case GxTextureFormat.ARGB8:
      case GxTextureFormat.INDEX4:
      case GxTextureFormat.INDEX8:
      case GxTextureFormat.INDEX14_X2:
      case GxTextureFormat.S3TC1:
        return ColorType.COLOR;

      default:
        throw new NotImplementedException();
    }
  }

  public float MinLod => this.Header.MinLodTimes8 / 8f;
  public float MaxLod => this.Header.MaxLodTimes8 / 8f;
  public float LodBias => this.Header.LodBiasTimes100 / 100f;

  public static bool operator==(BmdGxTexture lhs, BmdGxTexture rhs)
    => lhs.Equals(rhs);

  public static bool operator!=(BmdGxTexture lhs, BmdGxTexture rhs)
    => !lhs.Equals(rhs);

  public override bool Equals(object? obj) {
    if (Object.ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj is BmdGxTexture other) {
      return this.Name.Equals(other.Name) &&
             this.MipmapImages.SequenceEqual(other.MipmapImages) &&
             WrapModeS == other.WrapModeS &&
             WrapModeT == other.WrapModeT &&
             MinTextureFilter == other.MinTextureFilter &&
             MagTextureFilter == other.MagTextureFilter &&
             ColorType == other.ColorType;
    }

    return false;
  }

  public override int GetHashCode()
    => FluentHash.Start()
                 .With(Name)
                 .With(MipmapImages)
                 .With(WrapModeS)
                 .With(WrapModeT)
                 .With(MinTextureFilter)
                 .With(MagTextureFilter)
                 .With(ColorType)
                 .Hash;
}