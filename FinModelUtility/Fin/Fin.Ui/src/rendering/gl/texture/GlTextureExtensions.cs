using fin.image;
using fin.image.formats;

using OpenTK.Graphics.OpenGL;

using SixLabors.ImageSharp.PixelFormats;

using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace fin.ui.rendering.gl.texture;

public static class GlTextureExtensions {
  public static unsafe IImage ConvertToImage(this IGlTexture texture,
                                             bool flipVertical = false) {
    var width = texture.Width;
    var height = texture.Height;

    var image = new Rgba32Image(width, height);

    using var fastLock = image.UnsafeLock();
    var pixelScan0 = fastLock.pixelScan0;
    if (!flipVertical) {
      ReadPixelsIntoDst_(texture, pixelScan0);
    } else {
      var temp = new Rgba32[width * height];
      fixed (Rgba32* tempPtr = &temp[0]) {
        ReadPixelsIntoDst_(texture, tempPtr);
      }

      var srcSpan = temp.AsSpan();
      var dstSpan = new Span<Rgba32>(pixelScan0, width * height);

      for (var srcY = 0; srcY < height; ++srcY) {
        var srcRow = GetRow_(srcSpan, texture, srcY);
        
        var dstY = height - 1 - srcY;
        var dstRow = GetRow_(dstSpan, texture, dstY);

        srcRow.CopyTo(dstRow);
      }
    }

    return image;
  }

  private static unsafe void ReadPixelsIntoDst_(IGlTexture texture,
                                                Rgba32* dst)
    => GL.GetTextureImage(texture.Id,
                          0,
                          PixelFormat.Rgba,
                          PixelType.UnsignedByte,
                          4 * texture.Width * texture.Height,
                          new IntPtr(dst));

  private static Span<Rgba32> GetRow_(Span<Rgba32> imageSpan,
                                      IGlTexture texture,
                                      int y)
    => imageSpan.Slice(y * texture.Width, texture.Width);
}