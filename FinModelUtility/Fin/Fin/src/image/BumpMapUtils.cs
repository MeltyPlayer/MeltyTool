using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image;

public static class BumpMapUtils {
  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://stackoverflow.com/questions/10652797/whats-the-logic-behind-creating-a-normal-map-from-a-texture
  /// </summary>
  public static unsafe Rgb24Image ConvertBumpMapImageToNormalImage(
      IReadOnlyImage image) {
    var normalImage =
        new Rgb24Image(PixelFormat.RGB888, image.Width, image.Height);
    using var normalImageLock = normalImage.UnsafeLock();
    var normalImageScan0 = normalImageLock.pixelScan0;

    image.Access(bumpGetHandler => {
      for (var y = 0; y < image.Height; ++y) {
        for (var x = 0; x < image.Width; ++x) {
          bumpGetHandler(
              x,
              y,
              out var centerIntensity,
              out _,
              out _,
              out _);

          byte leftIntensity;
          if (x > 0) {
            bumpGetHandler(
                x - 1,
                y,
                out leftIntensity,
                out _,
                out _,
                out _);
          } else {
            leftIntensity = centerIntensity;
          }

          byte rightIntensity;
          if (x < image.Width - 1) {
            bumpGetHandler(
                x + 1,
                y,
                out rightIntensity,
                out _,
                out _,
                out _);
          } else {
            rightIntensity = centerIntensity;
          }

          byte upIntensity;
          if (y > 0) {
            bumpGetHandler(
                x,
                y - 1,
                out upIntensity,
                out _,
                out _,
                out _);
          } else {
            upIntensity = centerIntensity;
          }

          byte downIntensity;
          if (y < image.Height - 1) {
            bumpGetHandler(
                x,
                y + 1,
                out downIntensity,
                out _,
                out _,
                out _);
          } else {
            downIntensity = centerIntensity;
          }

          var xIntensity
              = ((leftIntensity / 255f - rightIntensity / 255f + 1) * .5f) *
                255;
          var yIntensity
              = ((upIntensity / 255f - downIntensity / 255f + 1) * .5f) * 255;

          normalImageScan0[y * image.Width + x]
              = new Rgb24((byte) xIntensity, (byte) yIntensity, 255);
        }
      }
    });

    return normalImage;
  }
}