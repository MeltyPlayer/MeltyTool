using fin.color;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats;

public class IndexedImage16(
    PixelFormat pixelFormat,
    IImage<L16> impl,
    IColor[] palette)
    : BIndexedImage(pixelFormat,
                    impl,
                    palette) {
  public override unsafe void Access(IImage.AccessHandler accessHandler) {
    using var bytes = impl.UnsafeLock();
    var ptr = bytes.pixelScan0;

    void InternalGetHandler(
        int x,
        int y,
        out byte r,
        out byte g,
        out byte b,
        out byte a) {
      var index = ptr[y * this.Width + x];
      var color = this.Palette[index.PackedValue];
      r = color.Rb;
      g = color.Gb;
      b = color.Bb;
      a = color.Ab;
    }

    accessHandler(InternalGetHandler);
  }
}