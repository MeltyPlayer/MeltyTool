using fin.image;
using fin.image.formats;

using OpenTK.Graphics.ES30;

using SixLabors.ImageSharp.PixelFormats;

using PixelFormat = OpenTK.Graphics.ES30.PixelFormat;

namespace fin.ui.rendering.gl.texture;

public static class GlFboExtensions {
  public static unsafe IImage ConvertToImage(this GlFbo fbo,
                                             bool flipVertical = false) {
    var width = fbo.Width;
    var height = fbo.Height;

    var image = new Rgba32Image(width, height);

    using var fastLock = image.UnsafeLock();
    var pixelScan0 = fastLock.pixelScan0;
    if (!flipVertical) {
      ReadPixelsIntoDst_(fbo, pixelScan0);
    } else {
      var temp = new Rgba32[width * height];
      fixed (Rgba32* tempPtr = &temp[0]) {
        ReadPixelsIntoDst_(fbo, tempPtr);
      }

      var srcSpan = temp.AsSpan();
      var dstSpan = new Span<Rgba32>(pixelScan0, width * height);

      for (var srcY = 0; srcY < height; ++srcY) {
        var srcRow = GetRow_(srcSpan, fbo, srcY);

        var dstY = height - 1 - srcY;
        var dstRow = GetRow_(dstSpan, fbo, dstY);

        srcRow.CopyTo(dstRow);
      }
    }

    return image;
  }

  private static unsafe void ReadPixelsIntoDst_(GlFbo fbo,
                                                Rgba32* dst) {
    fbo.TargetFbo();
    GL.ReadPixels(0,
                  0,
                  fbo.Width,
                  fbo.Height,
                  PixelFormat.Rgba,
                  PixelType.UnsignedByte,
                  new IntPtr(dst));
    fbo.UntargetFbo();
  }

  private static Span<Rgba32> GetRow_(Span<Rgba32> imageSpan,
                                      GlFbo fbo,
                                      int y)
    => imageSpan.Slice(y * fbo.Width, fbo.Width);
}