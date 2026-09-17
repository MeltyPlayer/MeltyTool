using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Hashing;
using System.Linq;

using fin.color;
using fin.util.hash;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats;

public abstract class BIndexedImage<TIndexPixel>(
    PixelFormat pixelFormat,
    IImage<TIndexPixel> impl,
    IColor[] palette)
    : IImage 
    where TIndexPixel : unmanaged, IPixel<TIndexPixel> {
  ~BIndexedImage() => this.Dispose();

  public void Dispose() {
    impl.Dispose();
    GC.SuppressFinalize(this);
  }

  public IColor[] Palette { get; } = palette;
  public PixelFormat PixelFormat { get; } = pixelFormat;
  public int Width => impl.Width;
  public int Height => impl.Height;

  public abstract void Access(IImage.AccessHandler accessHandler);

  public bool HasAlphaChannel =>
      this.Palette.Any(color => Math.Abs(color.Af - 1) > .0001);

  public Bitmap AsBitmap() => FinImage.ConvertToBitmap(this);

  public void ExportToStream(Stream stream, LocalImageFormat imageFormat)
    => this.AsBitmap()
           .Save(stream,
                 imageFormat switch {
                     LocalImageFormat.BMP  => ImageFormat.Bmp,
                     LocalImageFormat.PNG  => ImageFormat.Png,
                     LocalImageFormat.JPEG => ImageFormat.Jpeg,
                     LocalImageFormat.GIF  => ImageFormat.Gif,
                     LocalImageFormat.WEBP => ImageFormat.Webp,
                 });

  public IImageLock<TIndexPixel> LockIndex() => impl.Lock();
  public FinUnsafeImageLock<TIndexPixel> UnsafeLockIndex() => impl.UnsafeLock();

  public override bool Equals(object? obj) {
    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj is IReadOnlyImage other) {
      return this.Equals(other);
    }

    return false;
  }

  public bool Equals(IReadOnlyImage? other) {
    if (other == null) {
      return false;
    }

    if (this.Width != other.Width ||
        this.Height != other.Height) {
      return false;
    }

    if (other is BIndexedImage<TIndexPixel> otherSame) {
      using var fastLock = this.LockIndex();
      var span = fastLock.Bytes;

      using var otherFastLock = otherSame.LockIndex();
      var otherSpan = otherFastLock.Bytes;

      return span.SequenceEqual(otherSpan) &&
             this.Palette.SequenceEqual(otherSame.Palette);
    }

    bool match = true;
    this.Access(thisAccessor => {
      other.Access(otherAccessor => {
        for (var y = 0; y < this.Height; ++y) {
          for (var x = 0; x < this.Width; ++x) {
            thisAccessor(x,
                         y,
                         out var thisR,
                         out var thisG,
                         out var thisB,
                         out var thisA);
            otherAccessor(x,
                          y,
                          out var otherR,
                          out var otherG,
                          out var otherB,
                          out var otherA);

            if (thisR != otherR ||
                thisG != otherG ||
                thisB != otherB ||
                thisA != otherA) {
              match = false;
              return;
            }
          }
        }
      });
    });

    return match;
  }

  private int? cachedHash_ = null;

  public override int GetHashCode() {
    if (this.cachedHash_ != null) {
      return this.cachedHash_.Value;
    }

    using var fastLock = this.LockIndex();
    var span = fastLock.Bytes;

    var hash = new FluentHash()
        .With((int) Crc32.HashToUInt32(span))
        .With(this.Palette);

    this.cachedHash_ = hash;
    return hash;
  }
}