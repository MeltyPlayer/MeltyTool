using fin.util.spans;

using OpenTK.Graphics.OpenGL4;

using readOnly;

namespace fin.ui.rendering.gl.buffers;

[GenerateReadOnly]
public partial interface IGlBuffer : IDisposable {
  int Id { get; }
  BufferTarget Target { get; }

  int GpuRangeOffset { get; }
  int GpuRangeLength { get; }
}

public interface IFixedSizeMutableGlBuffer : IGlBuffer {
  Span<byte> CpuBuffer { get; }
  void FlushToGpu();
}

public static partial class GlBuffer {
  public static IGlBuffer CreateStaticBuffer(
      BufferTarget target,
      ReadOnlySpan<byte> data)
    => new StaticGlBufferImpl(target, data);

  public static IFixedSizeMutableGlBuffer CreateFixedSizeMutableBuffer(
      BufferTarget target,
      int size)
    => OpenGlFeatureService.SupportsNamedBufferStorage
        ? new ModernFixedSizeMutableGlBufferImpl(target, size)
        : new LegacyFixedSizeMutableGlBufferImpl(target, size);

  private closed class BGlBufferImpl(BufferTarget target) : IGlBuffer {
    public abstract int Id { get; }
    public BufferTarget Target => target;

    public abstract int GpuRangeOffset { get; }
    public abstract int GpuRangeLength { get; }

    ~BGlBufferImpl() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      this.DisposeImpl();
      GL.DeleteBuffer(this.Id);
    }

    protected virtual void DisposeImpl() { }
  }

  private sealed class StaticGlBufferImpl : BGlBufferImpl {
    public override int Id { get; }

    public override int GpuRangeOffset { get; }
    public override int GpuRangeLength { get; }

    public unsafe StaticGlBufferImpl(BufferTarget target,
                                     ReadOnlySpan<byte> data) : base(target) {
      this.GpuRangeOffset = 0;
      this.GpuRangeLength = data.Length;

      fixed (byte* ptr = &data[0]) {
        if (OpenGlFeatureService.SupportsNamedBufferStorage) {
          GL.CreateBuffers(1, out int id);
          this.Id = id;
          GL.NamedBufferStorage(
              id,
              data.Length,
              (nint) ptr,
              BufferStorageFlags.None);
        } else {
          var id = this.Id = GL.GenBuffer();
          GL.BindBuffer(target, id);
          GL.BufferData(
              target,
              data.Length,
              (nint) ptr,
              BufferUsageHint.StaticDraw);
        }
      }
    }
  }

  private class LegacyFixedSizeMutableGlBufferImpl
      : BGlBufferImpl, IFixedSizeMutableGlBuffer {
    private readonly byte[] previousData_;
    private readonly byte[] buffer_;

    public override int Id { get; }

    public override int GpuRangeOffset { get; }
    public override int GpuRangeLength { get; }

    public LegacyFixedSizeMutableGlBufferImpl(
        BufferTarget target,
        int size) : base(target) {
      this.GpuRangeOffset = 0;
      this.GpuRangeLength = size;

      var id = this.Id = GL.GenBuffer();
      this.previousData_ = new byte[size];
      this.buffer_ = new byte[size];

      GL.BindBuffer(target, id);
      GL.BufferData(target,
                    size,
                    this.previousData_,
                    BufferUsageHint.StreamDraw);

      this.previousData_ = new byte[size];
    }

    public Span<byte> CpuBuffer => this.buffer_;

    public void FlushToGpu() {
      if (this.previousData_.SequenceEqual(this.buffer_)) {
        return;
      }

      this.buffer_.CopyTo(this.previousData_);

      GL.BindBuffer(this.Target, this.Id);
      GL.BufferSubData(
          this.Target,
          0,
          this.buffer_.Length,
          this.buffer_);
    }
  }
}