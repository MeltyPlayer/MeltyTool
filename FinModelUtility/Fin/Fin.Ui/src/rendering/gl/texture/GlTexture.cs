using System.Buffers;
using System.Runtime.CompilerServices;

using fin.data;
using fin.image;
using fin.image.formats;
using fin.model;

using OpenTK.Graphics.OpenGL4;

using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace fin.ui.rendering.gl.texture;

public record GlTextureParams {
  public static GlTextureParams FromTexture(IReadOnlyTexture texture)
    => new() {
        Image = texture.Image,
        MipmapImages = texture.MipmapImages,
    };

  public required IReadOnlyImage Image { get; init; }
  public required IReadOnlyList<IReadOnlyImage> MipmapImages { get; init; }
}

public sealed class GlTexture : IGlTexture {
  // Intentionally separates params from texture, so we can share a single GL
  // texture between multiple Fin textures.
  private static ReferenceCountCacheDictionary<GlTextureParams, GlTexture>
      cache_ = new(
          prms => new GlTexture(prms),
          (_, glTexture) => {
            var id = glTexture.Id;
            if (id != UNDEFINED_ID) {
              GL.DeleteTextures(1, ref id);
              glTexture.Id = id;
              glTexture.IsDisposed = true;
            }
          },
          count => DebugService.OpenGlTextureCount = count);

  private const int UNDEFINED_ID = -1;
  private readonly GlTextureParams? params_;

  private bool canGenerateMipmaps_;

  public static GlTexture FromTexture(IReadOnlyTexture texture)
    => FromParams(GlTextureParams.FromTexture(texture));

  public static GlTexture FromParams(GlTextureParams prms)
    => cache_.GetAndIncrement(prms);

  public GlTexture(IReadOnlyImage image) {
    GL.GenTextures(1, out int id);
    this.Id = id;

    var target = TextureTarget.Texture2D;
    GL.BindTexture(target, this.Id);
    GlUtil.BindTexture(0, this.Id);
    this.LoadImageIntoTexture_(image, 0);
  }

  private GlTexture(GlTextureParams prms) {
    this.params_ = prms;

    GL.GenTextures(1, out int id);
    this.Id = id;

    var target = TextureTarget.Texture2D;
    GlUtil.BindTexture(0, this.Id);
    {
      var mipmapImages = prms.MipmapImages;

      this.LoadMipmapImagesIntoTexture_(mipmapImages);
      if (mipmapImages.Count > 1) {
        GL.TexParameter(target,
                        TextureParameterName.TextureMaxLevel,
                        mipmapImages.Count - 1);
      } else {
        this.canGenerateMipmaps_ = true;
      }
    }
  }

  public void GenerateMipmapsIfHaventYet() {
    if (this.canGenerateMipmaps_) {
      this.canGenerateMipmaps_ = false;
      GlUtil.BindTexture(0, this.Id);
      GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }
  }

  private static readonly MemoryPool<byte> pool_ = MemoryPool<byte>.Shared;

  private void LoadMipmapImagesIntoTexture_(
      IReadOnlyList<IReadOnlyImage> mipmapImages) {
    for (var i = 0; i < mipmapImages.Count; ++i) {
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
                            PixelInternalFormat.Rgba8,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Rgba,
                            fastLock.byteScan0);
        break;
      }
      case Rgb24Image rgb24Image: {
        using var fastLock = rgb24Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            PixelInternalFormat.Rgb8,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Rgb,
                            fastLock.byteScan0);
        break;
      }
      // TODO: Luminance/LuminanceAlpha is not supported in OpenGL ES. Implement support for R/RG instead
      /*
      case La16Image la16Image: {
        using var fastLock = la16Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            TextureComponentCount.LuminanceAlpha,
                            imageWidth,
                            imageHeight,
                            PixelFormat.LuminanceAlpha,
                            fastLock.byteScan0);
        break;
      }
      case L8Image l8Image: {
        using var fastLock = l8Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            TextureComponentCount.Luminance,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Luminance,
                            fastLock.byteScan0);
        break;
      }
      case I8Image i8Image: {
        using var fastLock = i8Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            TextureComponentCount.LuminanceAlpha,
                            imageWidth,
                            imageHeight,
                            PixelFormat.LuminanceAlpha,
                            fastLock.byteScan0);
        break;
      }*/
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
                            PixelInternalFormat.Rgba8,
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
      PixelInternalFormat internalFormat,
      int imageWidth,
      int imageHeight,
      PixelFormat pixelFormat,
      byte* scan0) {
    // This is required to fix a rare issue with alignment:
    // https://stackoverflow.com/questions/52460143/texture-not-showing-correctly
    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
    GL.TexImage2D(TextureTarget.Texture2D,
                  level,
                  internalFormat,
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
    if (this.IsDisposed) {
      return;
    }

    if (this.params_ != null) {
      cache_.DecrementAndMaybeDispose(this.params_);
    }
  }

  public int Id { get; private set; } = UNDEFINED_ID;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int textureIndex = 0)
    => GlUtil.BindTexture(textureIndex, this.Id);
}