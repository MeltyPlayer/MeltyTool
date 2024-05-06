using fin.image;
using fin.image.formats;

namespace fin.util.image {
  public enum ImageTransparencyType {
    OPAQUE,
    MASK,
    TRANSPARENT,
  }

  public static class ImageUtil {
    public static ImageTransparencyType GetTransparencyType(
        IReadOnlyImage image) {
      if (!image.HasAlphaChannel) {
        return ImageTransparencyType.OPAQUE;
      }

      switch (image) {
        case La16Image la16Image: {
          using var imgLock = la16Image.Lock();

          var transparencyType = ImageTransparencyType.OPAQUE;
          foreach (var pixel in imgLock.Pixels) {
            switch (pixel.A) {
              case 0: {
                transparencyType = ImageTransparencyType.MASK;
                break;
              }
              case < 255: {
                return ImageTransparencyType.TRANSPARENT;
              }
            }
          }

          return transparencyType;
        }
        case Rgba32Image rgba32Image: {
          using var imgLock = rgba32Image.Lock();

          var transparencyType = ImageTransparencyType.OPAQUE;
          foreach (var pixel in imgLock.Pixels) {
            switch (pixel.A) {
              case 0: {
                transparencyType = ImageTransparencyType.MASK;
                break;
              }
              case < 255: {
                return ImageTransparencyType.TRANSPARENT;
              }
            }
          }

          return transparencyType;
        }
      }

      {
        var transparencyType = ImageTransparencyType.OPAQUE;
        image.Access(
            getHandler => {
              for (var y = 0; y < image.Height; ++y) {
                for (var x = 0; x < image.Width; ++x) {
                  getHandler(x, y, out _, out _, out _, out var a);
                  switch (a) {
                    case 0: {
                      transparencyType = ImageTransparencyType.MASK;
                      break;
                    }
                    case < 255: {
                      transparencyType = ImageTransparencyType.TRANSPARENT;
                      return;
                    }
                  }
                }
              }
            });

        return transparencyType;
      }
    }
  }
}