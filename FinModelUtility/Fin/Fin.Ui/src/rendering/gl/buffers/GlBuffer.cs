using fin.util.spans;

using OpenTK.Graphics.OpenGL4;

using readOnly;

namespace fin.ui.rendering.gl.buffers;

[GenerateReadOnly]
public partial interface IGlBuffer : IDisposable {
  int Id { get; }
  BufferTarget Target { get; }
}

public interface IFixedSizeMutableGlBuffer : IGlBuffer {
  WriteOnlySpan<byte> Buffer { get; }
  void Flush();
}

public static class GlBuffer {
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

    ~BGlBufferImpl() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() => GL.DeleteBuffer(this.Id);
  }

  private sealed class StaticGlBufferImpl : BGlBufferImpl {
    public override int Id { get; }

    public unsafe StaticGlBufferImpl(BufferTarget target,
                                     ReadOnlySpan<byte> data) : base(target) {
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

  private class ModernFixedSizeMutableGlBufferImpl
      : BGlBufferImpl, IFixedSizeMutableGlBuffer {
    private readonly byte* bufferPtr_;
    private readonly int bufferLength_;

    public override int Id { get; }

    public unsafe ModernFixedSizeMutableGlBufferImpl(
        BufferTarget target,
        int size) : base(target) {
      GL.CreateBuffers(1, out int id);
      this.Id = id;

      this.bufferLength_ = size;

      GL.NamedBufferStorage(
          id,
          size,
          0,
          BufferStorageFlags.MapWriteBit |
          BufferStorageFlags.MapPersistentBit |
          BufferStorageFlags.MapCoherentBit);

      this.bufferPtr_ = (byte*) GL.MapNamedBuffer(
          this.Id,
          BufferAccess.WriteOnly);
    }

    public unsafe WriteOnlySpan<byte> Buffer
      => new(this.bufferPtr_, this.bufferLength_);

    public void Flush() { }
  }

  private class LegacyFixedSizeMutableGlBufferImpl
      : BGlBufferImpl, IFixedSizeMutableGlBuffer {
    private readonly byte[] previousData_;
    private readonly byte[] buffer_;

    public override int Id { get; }

    public LegacyFixedSizeMutableGlBufferImpl(
        BufferTarget target,
        int size) : base(target) {
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

    public WriteOnlySpan<byte> Buffer => this.buffer_;

    public void Flush() {
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