using fin.image;

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