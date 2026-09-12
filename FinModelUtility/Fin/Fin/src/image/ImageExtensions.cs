using System;
using System.Numerics;

using fin.image.formats;
using fin.math;

using readOnly;

using SixLabors.ImageSharp.PixelFormats;

using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace fin.image;

public static class ImageExtensions {
  public static IImage RemoveTopLeftBackgroundColor(this IReadOnlyImage src) {
    Color topLeftBackgroundColor = default;
    src.Access(getHandler => {
      getHandler(0, 0, out var r, out var g, out var b, out _);
      topLeftBackgroundColor = Color.FromArgb(r, g, b);
    });
    return src.RemoveBackgroundColor(topLeftBackgroundColor);
  }

  public static unsafe IImage RemoveBackgroundColor(this IReadOnlyImage src,
                                                    Color color) {
    var width = src.Width;
    var height = src.Height;

    var textureImageWithAlpha = new Rgba32Image(src.PixelFormat, width, height);
    using var alphaLock = textureImageWithAlpha.UnsafeLock();
    var alphaScan0 = alphaLock.pixelScan0;

    src.Access(getHandler => {
      for (var y = 0; y < height; ++y) {
        for (var x = 0; x < width; ++x) {
          getHandler(x,
                     y,
                     out var r,
                     out var g,
                     out var b,
                     out var a);

          if (r == color.R && g == color.G && b == color.B) {
            a = 0;
          }

          alphaScan0[y * width + x] = new Rgba32(r, g, b, a);
        }
      }
    });

    return textureImageWithAlpha;
  }

  public static IImage SubImage(this IReadOnlyImage src, Rectangle region) {
    var dst = new Rgba32Image(src.PixelFormat, region.Width, region.Height);

    var dstLock = dst.Lock();

    src.Access(getHandler => {
      for (var dstY = 0; dstY < dst.Height; ++dstY) {
        var srcY = region.Y + dstY;

        for (var dstX = 0; dstX < dst.Width; ++dstX) {
          var srcX = region.X + dstX;

          getHandler(srcX, srcY, out var r, out var g, out var b, out var a);

          var dstI = dstY * dst.Width + dstX;
          dstLock.Pixels[dstI] = new Rgba32(r, g, b, a);
        }
      }
    });

    return dst;
  }

  public delegate void Rgba32GetInterpolatedHandler(
      float x,
      float y,
      out float r,
      out float g,
      out float b,
      out float a);

  public delegate void AccessInterpolatedHandler(
      Rgba32GetInterpolatedHandler getHandler);

  public static void AccessInterpolated(
      this IReadOnlyImage image,
      AccessInterpolatedHandler accessHandler) {
    var imageWidth = image.Width;
    var imageHeight = image.Height;

    image.Access(getHandler => {
      void InternalGetInterpolatedHandler(
          float x,
          float y,
          out float r,
          out float g,
          out float b,
          out float a) {
        var leftX = (int) Math.Floor(x).Clamp(0, imageWidth - 1);
        var topY = (int) Math.Floor(y).Clamp(0, imageHeight - 1);
        var rightX = (int) (x + 1).Clamp(0, imageWidth - 1);
        var bottomY = (int) (y + 1).Clamp(0, imageHeight - 1);

        var xF = x - leftX;
        var yF = y - topY;

        getHandler(
            leftX,
            topY,
            out var rTopLeft,
            out var gTopLeft,
            out var bTopLeft,
            out var aTopLeft);
        getHandler(
            rightX,
            topY,
            out var rTopRight,
            out var gTopRight,
            out var bTopRight,
            out var aTopRight);
        getHandler(
            rightX,
            bottomY,
            out var rBottomRight,
            out var gBottomRight,
            out var bBottomRight,
            out var aBottomRight);
        getHandler(
            leftX,
            bottomY,
            out var rBottomLeft,
            out var gBottomLeft,
            out var bBottomLeft,
            out var aBottomLeft);

        r = (1 - yF) * ((1 - xF) * rTopLeft + xF * rTopRight) +
            yF * ((1 - xF) * rBottomLeft + xF * rBottomRight);
        g = (1 - yF) * ((1 - xF) * gTopLeft + xF * gTopRight) +
            yF * ((1 - xF) * gBottomLeft + xF * gBottomRight);
        b = (1 - yF) * ((1 - xF) * bTopLeft + xF * bTopRight) +
            yF * ((1 - xF) * bBottomLeft + xF * bBottomRight);
        a = (1 - yF) * ((1 - xF) * aTopLeft + xF * aTopRight) +
            yF * ((1 - xF) * aBottomLeft + xF * aBottomRight);
      }

      accessHandler(InternalGetInterpolatedHandler);
    });
  }
}