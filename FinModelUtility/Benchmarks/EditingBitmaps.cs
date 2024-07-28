using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;

using BenchmarkDotNet.Attributes;

using FastBitmapLib;

using fin.image.formats;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace benchmarks {
  public unsafe class EditingBitmaps {
    private const int SIZE = 4000;

    private Bitmap bitmap_ =
        new(SIZE, SIZE, PixelFormat.Format32bppArgb);

    private BitmapData bmpData_;

    public static readonly Configuration ImageSharpConfig;

    private Rgba32Image finImage_
        = new(fin.image.PixelFormat.RGBA8888, SIZE, SIZE);

    static EditingBitmaps() {
      ImageSharpConfig = Configuration.Default.Clone();
      ImageSharpConfig.PreferContiguousImageBuffers = true;
    }

    private Image<Rgba32> image_ = new(ImageSharpConfig, SIZE, SIZE);

    private MemoryHandle memoryHandle_;
    private Rgba32* imagePtr_;

    public void LockBitmap() {
      this.bmpData_ = this.bitmap_.LockBits(
          new Rectangle(0, 0, SIZE, SIZE),
          ImageLockMode.ReadWrite,
          PixelFormat.Format32bppArgb);
    }

    public void UnlockBitmap() {
      this.bitmap_.UnlockBits(this.bmpData_);
    }

    public IDisposable LockImage() {
      var frame = this.image_.Frames[0];
      frame.DangerousTryGetSinglePixelMemory(out var memory);

      this.memoryHandle_ = memory.Pin();
      this.imagePtr_ = (Rgba32*) this.memoryHandle_.Pointer;

      return this.memoryHandle_;
    }

    [Benchmark]
    public void ReadingBitmapBytes() {
      this.LockBitmap();
      var ptr = (byte*) this.bmpData_.Scan0;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var i = 4 * (y * SIZE + x);
          var b = ptr[i + 0];
          var g = ptr[i + 1];
          var r = ptr[i + 2];
          var a = ptr[i + 3];
        }
      }

      this.UnlockBitmap();
    }

    [Benchmark]
    public void ReadingBitmapUints() {
      this.LockBitmap();
      var ptr = (uint*) this.bmpData_.Scan0;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var i = y * SIZE + x;
          var bgra = ptr[i];
          var b = bgra & 0xff;
          var g = (bgra >> 8) & 0xff;
          var r = (bgra >> 16) & 0xff;
          var a = bgra >> 24;
        }
      }

      this.UnlockBitmap();
    }

    [Benchmark]
    public void ReadingBitmapUintsAndCasting() {
      this.LockBitmap();
      var ptr = (uint*) this.bmpData_.Scan0;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var i = y * SIZE + x;

          var bgra = ptr[i];
          var b = (byte) bgra;
          var g = (byte) (bgra >> 8);
          var r = (byte) (bgra >> 16);
          var a = (byte) (bgra >> 24);
        }
      }

      this.UnlockBitmap();
    }

    [Benchmark]
    public void ReadingBitmapUintsViaFastBitmapLibGetPixel() {
      using var fastBitmap = this.bitmap_.FastLock();
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var bgra = fastBitmap.GetPixelUInt(x, y);
          var b = bgra & 0xff;
          var g = (bgra >> 8) & 0xff;
          var r = (bgra >> 16) & 0xff;
          var a = bgra >> 24;
        }
      }
    }

    [Benchmark]
    public void ReadingBitmapUintsViaFastBitmapLibPtr() {
      using var fastBitmap = this.bitmap_.FastLock();
      var ptr = (uint*) fastBitmap.Scan0;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var i = y * SIZE + x;

          var bgra = ptr[i];
          var b = bgra & 0xff;
          var g = (bgra >> 8) & 0xff;
          var r = (bgra >> 16) & 0xff;
          var a = bgra >> 24;
        }
      }
    }

    [Benchmark]
    public void ReadingBitmapUintsSchenanigans() {
      this.LockBitmap();
      var ptr = (uint*) this.bmpData_.Scan0;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var bgra = *(ptr++);
          var b = (byte) bgra;
          var g = (byte) (bgra >> 8);
          var r = (byte) (bgra >> 16);
          var a = (byte) (bgra >> 24);
        }
      }

      this.UnlockBitmap();
    }

    //[Benchmark]
    public void ReadingBitmapColors() {
      this.LockBitmap();
      var ptr = (int*) this.bmpData_.Scan0;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var i = y * SIZE + x;
          var color = Color.FromArgb(ptr[i]);
          var r = color.R;
          var g = color.G;
          var b = color.B;
          var a = color.A;
        }
      }

      this.UnlockBitmap();
    }

    //[Benchmark]
    public void ReadingImageBytes() {
      using var _ = this.LockImage();
      var ptr = (byte*) this.imagePtr_;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var i = 4 * (y * SIZE + x);
          var r = ptr[i + 0];
          var g = ptr[i + 1];
          var b = ptr[i + 2];
          var a = ptr[i + 3];
        }
      }
    }

    [Benchmark]
    public void ReadingImageByteViaHandler() {
      using var _ = this.LockImage();
      var ptr = (byte*) this.imagePtr_;
      var handler = (int x,
                     int y,
                     out byte r,
                     out byte g,
                     out byte b,
                     out byte a)
                        => {
                      var value = ptr[y * SIZE + x];
                      r = (byte) (value & 0xff);
                      g = (byte) ((value >> 8) & 0xff);
                      b = (byte) ((value >> 16) & 0xff);
                      a = (byte) (value >> 24);
                    };

      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          handler(x, y, out var r, out var g, out var b, out var a);
        }
      }
    }

    [Benchmark]
    public void ReadingImageUints() {
      using var _ = this.LockImage();
      var ptr = (uint*) this.imagePtr_;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var i = y * SIZE + x;
          var bgra = ptr[i];
          var b = bgra & 0xff;
          var g = (bgra >> 8) & 0xff;
          var r = (bgra >> 16) & 0xff;
          var a = bgra >> 24;
        }
      }
    }

    //[Benchmark]
    public void ReadingImageRgba32s() {
      using var _ = this.LockImage();
      var ptr = this.imagePtr_;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var i = y * SIZE + x;
          var rgba = ptr[i];
          var r = rgba.R;
          var g = rgba.G;
          var b = rgba.B;
          var a = rgba.A;
        }
      }
    }

    [Benchmark]
    public void ReadingFinImageWithNewValues() {
      this.finImage_.Access(get => {
                         for (var y = 0; y < SIZE; ++y) {
                           for (var x = 0; x < SIZE; ++x) {
                             get(x,
                                 y,
                                 out var r,
                                 out var g,
                                 out var b,
                                 out var a);
                           }
                         }
                       });
    }

    [Benchmark]
    public void ReadingFinImageWithSameValues() {
      this.finImage_.Access(get => {
                         byte r, g, b, a;
                         for (var y = 0; y < SIZE; ++y) {
                           for (var x = 0; x < SIZE; ++x) {
                             get(x, y, out r, out g, out b, out a);
                           }
                         }
                       });
    }

    [Benchmark]
    public void ReadingFinImageWithLock() {
      using var imageLock = this.finImage_.Lock();
      var ptr = imageLock.Pixels;
      for (var y = 0; y < SIZE; ++y) {
        for (var x = 0; x < SIZE; ++x) {
          var i = y * SIZE + x;
          var rgba = ptr[i];
          var r = rgba.R;
          var g = rgba.G;
          var b = rgba.B;
          var a = rgba.A;
        }
      }
    }
  }
}