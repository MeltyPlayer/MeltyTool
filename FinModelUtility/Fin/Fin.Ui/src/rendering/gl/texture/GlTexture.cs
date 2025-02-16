using System.Buffers;
using System.Runtime.CompilerServices;

using fin.image;
using fin.image.formats;
using fin.model;

using OpenTK.Graphics.OpenGL;

using FinTextureMinFilter = fin.model.TextureMinFilter;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using TextureMagFilter = fin.model.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.OpenGL.TextureMinFilter;


namespace fin.ui.rendering.gl.texture;

public class GlTexture : IGlTexture {
  private static readonly Dictionary<IReadOnlyTexture, GlTexture> cache_
      = new();


  private const int UNDEFINED_ID = -1;
  private int id_ = UNDEFINED_ID;
  private readonly IReadOnlyTexture texture_;

  public static GlTexture FromTexture(IReadOnlyTexture texture) {
    if (!cache_.TryGetValue(texture, out var glTexture)) {
      glTexture = new GlTexture(texture);
      cache_[texture] = glTexture;
    }

    return glTexture;
  }

  public GlTexture(IReadOnlyImage image) {
    GL.GenTextures(1, out int id);
    this.id_ = id;

    var target = TextureTarget.Texture2D;
    GL.BindTexture(target, this.id_);
    {
      this.LoadImageIntoTexture_(image, 0);
    }
    GL.BindTexture(target, UNDEFINED_ID);
  }

  private GlTexture(IReadOnlyTexture texture) {
    this.texture_ = texture;

    GL.GenTextures(1, out int id);
    this.id_ = id;

    var target = TextureTarget.Texture2D;
    GL.BindTexture(target, this.id_);
    {
      var mipmapImages = texture.MipmapImages;

      this.LoadMipmapImagesIntoTexture_(mipmapImages);

      if (mipmapImages.Length == 1 &&
          texture.MinFilter is FinTextureMinFilter.NEAR_MIPMAP_NEAR
                               or FinTextureMinFilter.NEAR_MIPMAP_LINEAR
                               or FinTextureMinFilter.LINEAR_MIPMAP_NEAR
                               or FinTextureMinFilter.LINEAR_MIPMAP_LINEAR) {
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
      } else {
        GL.TexParameter(target,
                        TextureParameterName.TextureMaxLevel,
                        mipmapImages.Length - 1);
      }

      var finBorderColor = texture.BorderColor;
      var hasBorderColor = finBorderColor != null;
      GL.TexParameter(target,
                      TextureParameterName.TextureWrapS,
                      (int) ConvertFinWrapToGlWrap_(
                          texture.WrapModeU,
                          hasBorderColor));
      GL.TexParameter(target,
                      TextureParameterName.TextureWrapT,
                      (int) ConvertFinWrapToGlWrap_(
                          texture.WrapModeV,
                          hasBorderColor));

      if (hasBorderColor) {
        var glBorderColor = new[] {
            finBorderColor.Rf,
            finBorderColor.Gf,
            finBorderColor.Bf,
            finBorderColor.Af
        };

        GL.TexParameter(target,
                        TextureParameterName.TextureBorderColor,
                        glBorderColor);
      }

      GL.TexParameter(
          target,
          TextureParameterName.TextureMinFilter,
          (int) (texture.MinFilter switch {
              FinTextureMinFilter.NEAR   => TextureMinFilter.Nearest,
              FinTextureMinFilter.LINEAR => TextureMinFilter.Linear,
              FinTextureMinFilter.NEAR_MIPMAP_NEAR => TextureMinFilter
                  .NearestMipmapNearest,
              FinTextureMinFilter.NEAR_MIPMAP_LINEAR => TextureMinFilter
                  .NearestMipmapLinear,
              FinTextureMinFilter.LINEAR_MIPMAP_NEAR => TextureMinFilter
                  .LinearMipmapNearest,
              FinTextureMinFilter.LINEAR_MIPMAP_LINEAR => TextureMinFilter
                  .LinearMipmapLinear,
          }));
      GL.TexParameter(
          target,
          TextureParameterName.TextureMagFilter,
          (int) (texture.MagFilter switch {
              TextureMagFilter.NEAR => OpenTK.Graphics.OpenGL.TextureMagFilter
                                             .Nearest,
              TextureMagFilter.LINEAR => OpenTK.Graphics.OpenGL
                                               .TextureMagFilter.Linear,
              _ => throw new ArgumentOutOfRangeException()
          }));
      GL.TexParameter(target,
                      TextureParameterName.TextureLodBias,
                      texture.LodBias);
      GL.TexParameter(target,
                      TextureParameterName.TextureMinLod,
                      texture.MinLod);
      GL.TexParameter(target,
                      TextureParameterName.TextureMaxLod,
                      texture.MaxLod);
    }
    GL.BindTexture(target, UNDEFINED_ID);
  }

  private static readonly MemoryPool<byte> pool_ = MemoryPool<byte>.Shared;

  private void LoadMipmapImagesIntoTexture_(IReadOnlyImage[] mipmapImages) {
    for (var i = 0; i < mipmapImages.Length; ++i) {
      this.LoadImageIntoTexture_(mipmapImages[i], i);
    }
  }

  private unsafe void LoadImageIntoTexture_(IReadOnlyImage image, int level) {
    var imageWidth = image.Width;
    var imageHeight = image.Height;

    switch (image) {
      case Rgba32Image rgba32Image: {
        using var fastLock = rgba32Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            PixelInternalFormat.Rgba,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Rgba,
                            fastLock.byteScan0);
        break;
      }
      case Rgb24Image rgb24Image: {
        using var fastLock = rgb24Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            PixelInternalFormat.Rgb,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Rgb,
                            fastLock.byteScan0);
        break;
      }
      case La16Image ia16Image: {
        using var fastLock = ia16Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            PixelInternalFormat.LuminanceAlpha,
                            imageWidth,
                            imageHeight,
                            PixelFormat.LuminanceAlpha,
                            fastLock.byteScan0);
        break;
      }
      case L8Image i8Image: {
        using var fastLock = i8Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            PixelInternalFormat.Luminance,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Luminance,
                            fastLock.byteScan0);
        break;
      }
      default: {
        using var rentedBytes = pool_.Rent(4 * imageWidth * imageHeight);
        image.Access(getHandler => {
          var pixelBytes = rentedBytes.Memory.Span;
          for (var y = 0; y < imageHeight; y++) {
            for (var x = 0; x < imageWidth; x++) {
              getHandler(x,
                         y,
                         out var r,
                         out var g,
                         out var b,
                         out var a);

              var outI = 4 * (y * imageWidth + x);  
              pixelBytes[outI] = r;
              pixelBytes[outI + 1] = g;
              pixelBytes[outI + 2] = b;
              pixelBytes[outI + 3] = a;
            }
          }
        });
        PassBytesIntoImage_(level,
                            PixelInternalFormat.Rgba,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Rgba,
                            (byte*) rentedBytes.Memory.Pin().Pointer);
        break;
      }
    }
  }

  private static unsafe void PassBytesIntoImage_(
      int level,
      PixelInternalFormat pixelInternalFormat,
      int imageWidth,
      int imageHeight,
      PixelFormat pixelFormat,
      byte* scan0) {
    // This is required to fix a rare issue with alignment:
    // https://stackoverflow.com/questions/52460143/texture-not-showing-correctly
    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
    GL.TexImage2D(TextureTarget.Texture2D,
                  level,
                  pixelInternalFormat,
                  imageWidth,
                  imageHeight,
                  0,
                  pixelFormat,
                  PixelType.UnsignedByte,
                  (IntPtr) scan0);
  }

  ~GlTexture() => this.ReleaseUnmanagedResources_();

  public bool IsDisposed { get; private set; }

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.IsDisposed = true;
    cache_.Remove(this.texture_);

    var id = this.id_;
    GL.DeleteTextures(1, ref id);

    this.id_ = UNDEFINED_ID;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int textureIndex = 0)
    => GlUtil.BindTexture(textureIndex, this.id_);

  private static TextureWrapMode ConvertFinWrapToGlWrap_(
      WrapMode wrapMode,
      bool hasBorderColor) =>
      wrapMode switch {
          WrapMode.CLAMP => hasBorderColor
              ? TextureWrapMode.ClampToBorder
              : TextureWrapMode.ClampToEdge,
          WrapMode.REPEAT => TextureWrapMode.Repeat,
          WrapMode.MIRROR_CLAMP or WrapMode.MIRROR_REPEAT
              => TextureWrapMode.MirroredRepeat,
          _ => throw new ArgumentOutOfRangeException(
              nameof(wrapMode),
              wrapMode,
              null)
      };
}