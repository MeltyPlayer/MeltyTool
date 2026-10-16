using fin.math;
using fin.util.spans;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl.buffers;

public static partial class GlBuffer {
  private class ModernFixedSizeMutableGlBufferImpl
      : BGlBufferImpl, IFixedSizeMutableGlBuffer {
    private readonly byte[] previousData_;
    private readonly byte[] scratchBuffer_;

    private const int BUFFER_COUNT_ = 5;
    private readonly int bufferLength_;
    private readonly int bufferLengthPadded_;

    private readonly byte* storagePtr_;

    private readonly GlFence?[] syncObjects_ = new GlFence?[BUFFER_COUNT_];

    private int CurrentGpuBufferIndex_ { get; set; } = BUFFER_COUNT_ - 1;

    private int CurrentCpuBufferIndex_
      => (this.CurrentGpuBufferIndex_ + 1) % BUFFER_COUNT_;

    public override int Id { get; }

    public override int GpuRangeOffset
      => this.CurrentGpuBufferIndex_ * this.bufferLengthPadded_;

    public override int GpuRangeLength => this.bufferLength_;

    public unsafe ModernFixedSizeMutableGlBufferImpl(
        BufferTarget target,
        int size) : base(target) {
      GL.CreateBuffers(1, out int id);
      this.Id = id;

      this.previousData_ = new byte[size];
      this.scratchBuffer_ = new byte[size];
      this.bufferLength_ = size;

      this.bufferLengthPadded_ = this.bufferLength_.CeilToNearest(256);

      var storageLength = BUFFER_COUNT_ * this.bufferLengthPadded_;
      GL.NamedBufferStorage(
          id,
          storageLength,
          0,
          BufferStorageFlags.MapWriteBit |
          BufferStorageFlags.MapPersistentBit |
          BufferStorageFlags.MapCoherentBit);

      this.storagePtr_ = (byte*) GL.MapNamedBufferRange(
          id,
          0,
          storageLength,
          BufferAccessMask.MapWriteBit |
          BufferAccessMask.MapPersistentBit |
          BufferAccessMask.MapCoherentBit);
    }

    protected virtual void DisposeImpl() {
      GL.UnmapNamedBuffer(this.Id);
    }

    public Span<byte> CpuBuffer => this.scratchBuffer_;

    public unsafe void FlushToGpu() {
      if (this.scratchBuffer_.SequenceEqual(this.previousData_)) {
        return;
      }

      var currentSyncObject = this.syncObjects_[this.CurrentCpuBufferIndex_];
      if (currentSyncObject != null) {
        currentSyncObject.Sync();
        currentSyncObject.RemoveOwner();
        this.syncObjects_[this.CurrentCpuBufferIndex_] = null;
      }

      this.scratchBuffer_.CopyTo(this.previousData_);

      var targetRegion = new Span<byte>(
          this.storagePtr_ +
          (this.CurrentCpuBufferIndex_ * this.bufferLengthPadded_),
          this.bufferLength_);
      this.scratchBuffer_.CopyTo(targetRegion);

      this.syncObjects_[this.CurrentCpuBufferIndex_]
          = GlUtil.RequestFenceForEndOfFrame();

      this.CurrentGpuBufferIndex_
          = (this.CurrentGpuBufferIndex_ + 1) % BUFFER_COUNT_;
    }
  }
}