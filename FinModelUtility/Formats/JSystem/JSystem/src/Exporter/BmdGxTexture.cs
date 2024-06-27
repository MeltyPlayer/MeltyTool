using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.image;
using fin.image.formats;
using fin.model;
using fin.util.hash;
using fin.util.image;

using gx;

using jsystem.schema.jutility.bti;

using SixLabors.ImageSharp.PixelFormats;

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

      var image = this.Header.ToBitmap();
      this.Image = image;

      // TODO: This is really weird, can this possibly be right???????
      if (false) {
        var alphaSetting = header.AlphaSetting;
        var alphaAlreadyGood = alphaSetting == JutTransparency.TRANSLUCENT;
        if (!alphaAlreadyGood &&
            alphaSetting is JutTransparency.OPAQUE or JutTransparency.CUTOUT) {
          var allGood = true;

          if (image.HasAlphaChannel) {
            image.Access(srcAccessor => {
              for (var y = 0; y < image.Height; y++) {
                for (var x = 0; x < image.Width; x++) {
                  srcAccessor(x, y, out _, out _, out _, out var a);

                  switch (alphaSetting) {
                    case JutTransparency.OPAQUE: {
                      if (a != 255) {
                        allGood = false;
                        return;
                      }

                      break;
                    }
                    case JutTransparency.CUTOUT: {
                      if (a != 0 && a != 255) {
                        allGood = false;
                        return;
                      }

                      break;
                    }
                    default: throw new ArgumentOutOfRangeException();
                  }
                }
              }
            });
          }

          alphaAlreadyGood = allGood;
        }

        // TODO: is this right????????
        // Process alpha channel with the image's alpha setting
        if (!alphaAlreadyGood) {
          var pixelCount = image.Width * image.Height;

        /*switch (alphaSetting) {
           case JutTransparency.OPAQUE: {
             switch (image) {
               case BIndexedImage bIndexedImage: {
                 var palette = bIndexedImage.Palette;
                 for (var i = 0; i < palette.Length; ++i) {
                   var color = palette[i];
                   palette[i] =
                       FinColor.FromRgbBytes(color.Rb, color.Gb, color.Bb);
                 }

                 break;
               }
               case La16Image la16Image: {
                 using var fastLock = la16Image.Lock();
                 var ptr = fastLock.pixelScan0;
                 for (var i = 0; i < pixelCount; ++i) {
                   var pixel = ptr[i];
                   pixel.A = 255;
                   ptr[i] = pixel;
                 }

                 break;
               }
               case Rgba32Image rgba32Image: {
                 using var fastLock = rgba32Image.Lock();
                 var ptr = fastLock.pixelScan0;
                 for (var i = 0; i < pixelCount; ++i) {
                   var pixel = ptr[i];
                   pixel.A = 255;
                   ptr[i] = pixel;
                 }

                 break;
               }
             }

             break;
           }
           case JutTransparency.CUTOUT: {
             switch (image) {
               case BIndexedImage bIndexedImage: {
                 var palette = bIndexedImage.Palette;
                 for (var i = 0; i < palette.Length; ++i) {
                   var color = palette[i];
                   palette[i] =
                       FinColor.FromRgbaBytes(color.Rb,
                                              color.Gb,
                                              color.Bb,
                                              CollapseAlphaForMask_(color.Ab));
                 }

                 break;
               }
               case La16Image la16Image: {
                 using var fastLock = la16Image.Lock();
                 var ptr = fastLock.pixelScan0;
                 for (var i = 0; i < pixelCount; ++i) {
                   var pixel = ptr[i];
                   pixel.A = CollapseAlphaForMask_(pixel.A);
                   ptr[i] = pixel;
                 }

                 break;
               }
               case Rgba32Image rgba32Image: {
                 using var fastLock = rgba32Image.Lock();
                 var ptr = fastLock.pixelScan0;
                 for (var i = 0; i < pixelCount; ++i) {
                   var pixel = ptr[i];
                   pixel.A = CollapseAlphaForMask_(pixel.A);
                   ptr[i] = pixel;
                 }

                 break;
               }
             }

             break;
           }
           case JutTransparency.SPECIAL: {
             switch (image) {
               case La16Image la16Image: {
                 using var fastLock = la16Image.Lock();
                 var ptr = fastLock.pixelScan0;
                 for (var i = 0; i < pixelCount; ++i) {
                   var pixel = ptr[i];
                   ptr[i] = new La16(pixel.A, pixel.L);
                 }

                 break;
               }
               // default: throw new NotImplementedException();
             }

             break;
           }
           default: throw new ArgumentOutOfRangeException();
         }*/

        switch (image) {
            case La16Image la16Image: {
              using var fastLock = la16Image.Lock();
              var ptr = fastLock.Pixels;
              for (var i = 0; i < pixelCount; ++i) {
                var pixel = ptr[i];
                ptr[i] = new La16(pixel.A, pixel.L);
              }

              break;
            }
          }
        }

        var type = TransparencyTypeUtil.GetTransparencyType(this.Image);
      }

      this.ColorType = BmdGxTexture.GetColorType_(this.Header.Format);
    }

  public string Name { get; }
  public override string ToString() => this.Name;

  public IImage Image { get; }
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

  public static bool operator ==(BmdGxTexture lhs, BmdGxTexture rhs)
    => lhs.Equals(rhs);

  public static bool operator !=(BmdGxTexture lhs, BmdGxTexture rhs)
    => !lhs.Equals(rhs);

  public override bool Equals(object? obj) {
      if (Object.ReferenceEquals(this, obj)) {
        return true;
      }

      if (obj is BmdGxTexture other) {
        return this.Name.Equals(other.Name) &&
               this.Image.Equals(other.Image) &&
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
                 .With(Image)
                 .With(WrapModeS)
                 .With(WrapModeT)
                 .With(MinTextureFilter)
                 .With(MagTextureFilter)
                 .With(ColorType)
                 .Hash;

  private static byte CollapseAlphaForMask_(byte value)
    => (byte) (value == 0 ? 0 : 255);
}