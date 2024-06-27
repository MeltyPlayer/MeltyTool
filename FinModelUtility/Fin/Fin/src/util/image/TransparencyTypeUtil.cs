using fin.image;
using fin.image.formats;
using fin.math.floats;

namespace fin.util.image;

public enum TransparencyType {
  OPAQUE,
  MASK,
  TRANSPARENT,
}

public static class TransparencyTypeUtil {
  public static TransparencyType GetTransparencyType(float value) {
    if (value.IsRoughly1()) {
      return TransparencyType.OPAQUE;
    }

    if (value.IsRoughly0()) {
      return TransparencyType.MASK;
    }

    return TransparencyType.TRANSPARENT;
  }

  public static TransparencyType GetTransparencyType(IReadOnlyImage image) {
    if (!image.HasAlphaChannel) {
      return TransparencyType.OPAQUE;
    }

    switch (image) {
      case La16Image la16Image: {
        using var imgLock = la16Image.Lock();

        var transparencyType = TransparencyType.OPAQUE;
        foreach (var pixel in imgLock.Pixels) {
          switch (pixel.A) {
            case 0: {
              transparencyType = TransparencyType.MASK;
              break;
            }
            case < 255: {
              return TransparencyType.TRANSPARENT;
            }
          }
        }

        return transparencyType;
      }
      case Rgba32Image rgba32Image: {
        using var imgLock = rgba32Image.Lock();

        var transparencyType = TransparencyType.OPAQUE;
        foreach (var pixel in imgLock.Pixels) {
          switch (pixel.A) {
            case 0: {
              transparencyType = TransparencyType.MASK;
              break;
            }
            case < 255: {
              return TransparencyType.TRANSPARENT;
            }
          }
        }

        return transparencyType;
      }
    }

    {
      var transparencyType = TransparencyType.OPAQUE;
      image.Access(
          getHandler => {
            for (var y = 0; y < image.Height; ++y) {
              for (var x = 0; x < image.Width; ++x) {
                getHandler(x, y, out _, out _, out _, out var a);
                switch (a) {
                  case 0: {
                    transparencyType = TransparencyType.MASK;
                    break;
                  }
                  case < 255: {
                    transparencyType = TransparencyType.TRANSPARENT;
                    return;
                  }
                }
              }
            }
          });

      return transparencyType;
    }
  }

  public static TransparencyType Merge(this TransparencyType lhs,
                                       TransparencyType rhs) {
    if (lhs == TransparencyType.TRANSPARENT ||
        rhs == TransparencyType.TRANSPARENT) {
      return TransparencyType.TRANSPARENT;
    }

    if (lhs == TransparencyType.MASK ||
        rhs == TransparencyType.MASK) {
      return TransparencyType.MASK;
    }

    return TransparencyType.OPAQUE;
  }
}