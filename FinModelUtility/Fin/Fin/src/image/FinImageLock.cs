using System;
using System.Buffers;

using CommunityToolkit.HighPerformance;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace fin.image;

/// <summary>
///   Based on how FastBitmap performs locking:
///   https://github.com/LuizZak/FastBitmap
/// </summary>
public unsafe struct FinImageLock<TPixel> : IImageLock<TPixel>
    where TPixel : unmanaged, IPixel<TPixel> {
  private bool isDisposed_ = false;
  private readonly MemoryHandle memoryHandle_;

  private readonly TPixel* pixelScan0_;
  private readonly int pixelCount_;

  public FinImageLock(Image<TPixel> image) {
    var frame = image.Frames[0];
    frame.DangerousTryGetSinglePixelMemory(out var memory);

    this.memoryHandle_ = memory.Pin();
    this.pixelScan0_ = (TPixel*) this.memoryHandle_.Pointer;
    this.pixelCount_ = image.Width * image.Height;
  }

  public void Dispose() {
    if (!this.isDisposed_) {
      this.isDisposed_ = true;
      this.memoryHandle_.Dispose();
    }
  }

  public Span<byte> Bytes => this.Pixels.AsBytes();
  public Span<TPixel> Pixels => new(this.pixelScan0_, this.pixelCount_);
}