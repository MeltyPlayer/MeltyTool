using System;
using System.Buffers;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace fin.image;

public unsafe struct FinUnsafeImageLock<TPixel> : IDisposable
    where TPixel : unmanaged, IPixel<TPixel> {
  private bool isDisposed_ = false;
  private readonly MemoryHandle memoryHandle_;

  public FinUnsafeImageLock(Image<TPixel> image) {
    var frame = image.Frames[0];
    frame.DangerousTryGetSinglePixelMemory(out var memory);

    this.memoryHandle_ = memory.Pin();
    this.byteScan0 = (byte*) this.memoryHandle_.Pointer;
    this.pixelScan0 = (TPixel*) this.byteScan0;
  }

  public void Dispose() {
    if (!this.isDisposed_) {
      this.isDisposed_ = true;
      this.memoryHandle_.Dispose();
    }
  }

  public readonly byte* byteScan0;
  public readonly TPixel* pixelScan0;
}