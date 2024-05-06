using System;
using System.Drawing;
using System.IO;
using System.IO.Hashing;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats {
  public abstract class BImage<TPixel> : IImage<TPixel>
      where TPixel : unmanaged, IPixel<TPixel> {
    protected BImage(PixelFormat format) {
      this.PixelFormat = format;
    }

    ~BImage() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() => this.Impl.Dispose();

    protected abstract Image<TPixel> Impl { get; }

    public PixelFormat PixelFormat { get; }
    public int Width => this.Impl.Width;
    public int Height => this.Impl.Height;

    public abstract void Access(IImage.AccessHandler accessHandler);
    public abstract bool HasAlphaChannel { get; }

    public Bitmap AsBitmap() => FinImage.ConvertToBitmap(this);

    public void ExportToStream(Stream stream, LocalImageFormat imageFormat)
      => this.Impl.Save(
          stream,
          FinImage.ConvertFinImageFormatToImageSharpEncoder(imageFormat));

    public IImageLock<TPixel> Lock() => new FinImageLock<TPixel>(Impl);
    public FinUnsafeImageLock<TPixel> UnsafeLock() => new(Impl);

    public override bool Equals(object? obj) {
      if (Object.ReferenceEquals(this, obj)) {
        return true;
      }

      if (obj is IImage otherGeneric) {
        if (this.Width != otherGeneric.Width ||
            this.Height != otherGeneric.Height) {
          return false;
        }

        if (obj is IImage<TPixel> otherSame) {
          using var fastLock = this.Lock();
          var span = fastLock.Bytes;

          using var otherFastLock = otherSame.Lock();
          var otherSpan = otherFastLock.Bytes;

          return span.SequenceEqual(otherSpan);
        }

        bool match = true;
        this.Access(
            thisAccessor => {
              otherGeneric.Access(
                  otherAccessor => {
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

      return false;
    }

    private int? cachedHash_ = null;

    public override unsafe int GetHashCode() {
      if (this.cachedHash_ != null) {
        return this.cachedHash_.Value;
      }

      using var fastLock = this.Lock();
      var span = fastLock.Bytes;

      var hash = (int) Crc32.HashToUInt32(span);
      this.cachedHash_ = hash;
      return hash;
    }
  }
}