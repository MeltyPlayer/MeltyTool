using BenchmarkDotNet.Attributes;

using fin.image;
using fin.image.formats;

using SixLabors.ImageSharp.PixelFormats;

namespace benchmarks;

public class ImageLoops {
  private const int N_ = 10000;
  private const int WIDTH_ = 64;
  private const int HEIGHT_ = 64;

  [Benchmark]
  public void WithTwoBoundsChecks() {
    using var img = new L8Image(PixelFormat.L8, WIDTH_, HEIGHT_);

    for (var n = 0; n < N_; n++) {
      var s = 0;

      using var imgLock = img.Lock();
      var scan0 = imgLock.Pixels;

      for (var y = 0; y < HEIGHT_; ++y) {
        for (var x = 0; x < WIDTH_; ++x) {
          scan0[y * WIDTH_ + x] = new L8((byte) s++);
        }
      }
    }
  }

  [Benchmark]
  public unsafe void UnsafeWithTwoBoundsChecksPixels() {
    using var img = new L8Image(PixelFormat.L8, WIDTH_, HEIGHT_);

    for (var n = 0; n < N_; n++) {
      var s = 0;

      using var unsafeLock = img.UnsafeLock();
      var scan0 = unsafeLock.pixelScan0;

      for (var y = 0; y < HEIGHT_; ++y) {
        for (var x = 0; x < WIDTH_; ++x) {
          scan0[y * WIDTH_ + x] = new L8((byte) s++);
        }
      }
    }
  }

  [Benchmark]
  public unsafe void UnsafeWithTwoBoundsChecksBytes() {
    using var img = new L8Image(PixelFormat.L8, WIDTH_, HEIGHT_);

    for (var n = 0; n < N_; n++) {
      var s = 0;

      using var unsafeLock = img.UnsafeLock();
      var scan0 = unsafeLock.byteScan0;

      for (var y = 0; y < HEIGHT_; ++y) {
        for (var x = 0; x < WIDTH_; ++x) {
          scan0[y * WIDTH_ + x] = (byte) s++;
        }
      }
    }
  }

  [Benchmark]
  public void WithOneBoundsCheck() {
    using var img = new L8Image(PixelFormat.L8, WIDTH_, HEIGHT_);

    for (var n = 0; n < N_; n++) {
      var s = 0;

      using var imgLock = img.Lock();
      var scan0 = imgLock.Pixels;

      for (var i = 0; i < scan0.Length; ++i) {
        scan0[i] = new L8((byte) s++);
      }
    }
  }

  [Benchmark]
  public unsafe void UnsafeWithOneBoundsCheck() {
    using var img = new L8Image(PixelFormat.L8, WIDTH_, HEIGHT_);

    for (var n = 0; n < N_; n++) {
      var s = 0;

      using var unsafeLock = img.UnsafeLock();
      var scan0 = unsafeLock.pixelScan0;

      var total = WIDTH_ * HEIGHT_;

      for (var i = 0; i < total; ++i) {
        scan0[i] = new L8((byte) s++);
      }
    }
  }
}