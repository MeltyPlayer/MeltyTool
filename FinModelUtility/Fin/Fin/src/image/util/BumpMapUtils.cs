using System;
using System.Numerics;

using fin.image.formats;
using fin.model;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.util;

public static class BumpMapUtils {
  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://stackoverflow.com/questions/10652797/whats-the-logic-behind-creating-a-normal-map-from-a-texture
  /// </summary>
  public static unsafe Rgb24Image ConvertBumpMapImageToNormalImage(
      IReadOnlyImage image,
      WrapMode wrapModeS,
      WrapMode wrapModeT) {
    var normalImage =
        new Rgb24Image(PixelFormat.RGB888, image.Width - 1, image.Height - 1);
    using var normalImageLock = normalImage.UnsafeLock();
    var normalImageScan0 = normalImageLock.pixelScan0;

    image.Access(bumpGetRgbaHandler => {
      Func<int, int, float> bumpGetHandler = (x, y) => {
        bumpGetRgbaHandler(x, y, out var value, out _, out _, out _);
        return value / 255f;
      };

      for (var y = 0; y < image.Height; ++y) {
        for (var x = 0; x < image.Width; ++x) {
          var leftX = wrapModeS.GetWrappedCoord(x - 1, image.Width);
          var rightX = wrapModeS.GetWrappedCoord(x + 1, image.Width);
          var upY = wrapModeT.GetWrappedCoord(y - 1, image.Height);
          var downY = wrapModeT.GetWrappedCoord(y + 1, image.Height);

          // Sample 3x3 neighborhood normalized to [0, 1]
          var tl = bumpGetHandler(leftX, upY);
          var t = bumpGetHandler(x, upY);
          var tr = bumpGetHandler(rightX, upY);

          var l = bumpGetHandler(leftX, y);
          var r = bumpGetHandler(rightX, y);

          var bl = bumpGetHandler(leftX, downY);
          var b = bumpGetHandler(x, downY);
          var br = bumpGetHandler(rightX, downY);

          // Sobel kernels (divided by 4 to match central-difference scale)
          var dx = ((tl + 2f * l + bl) - (tr + 2f * r + br)) * 0.25f;
          var dy = ((tl + 2f * t + tr) - (bl + 2f * b + br)) * 0.25f;
          var dz = 1f;

          var normal = Vector3.Normalize(new Vector3(dx, dy, dz));
          normalImageScan0[y * image.Width + x] = new Rgb24(
              (byte) ((normal.X * 0.5f + 0.5f) * 255f),
              (byte) ((normal.Y * 0.5f + 0.5f) * 255f),
              (byte) ((normal.Z * 0.5f + 0.5f) * 255f));
        }
      }
    });

    return normalImage;
  }
}