using fin.util.spans;

using OpenTK.Graphics.OpenGL4;

using readOnly;

using static fin.ui.rendering.gl.buffers.IFixedSizeMutableGlBuffer;

namespace fin.ui.rendering.gl.buffers;

[GenerateReadOnly]
public partial interface IGlBuffer : IDisposable {
  int Id { get; }
  BufferTarget Target { get; }
}

public interface IFixedSizeMutableGlBuffer : IGlBuffer {
  delegate void UpdateBuffer(WriteOnlySpan<byte> buffer);

  void UpdateData(UpdateBuffer updateBuffer);
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
    private readonly byte[] previousData_;
    private readonly byte* bufferPtr_;
    private readonly int bufferLength_;

    public override int Id { get; }

    public unsafe ModernFixedSizeMutableGlBufferImpl(
        BufferTarget target,
        int size) : base(target) {
      GL.CreateBuffers(1, out int id);
      this.Id = id;

      this.previousData_ = new byte[size];
      this.bufferLength_ = size;

      GL.NamedBufferStorage(
          id,
          size,
          0,
          BufferStorageFlags.MapWriteBit |
          BufferStorageFlags.MapPersistentBit);

      this.bufferPtr_ = (byte*) GL.MapNamedBuffer(
          this.Id,
          BufferAccess.WriteOnly);
    }

    public unsafe void UpdateData(UpdateBuffer updateBuffer) {
      Span<byte> scratchBuffer = stackalloc byte[this.bufferLength_];
      updateBuffer(scratchBuffer);

      if (scratchBuffer.SequenceEqual(this.previousData_)) {
        return;
      }

      var writeOnlySpan
          = new WriteOnlySpan<byte>(this.bufferPtr_, this.bufferLength_);
      scratchBuffer.CopyTo(writeOnlySpan);
    }
  }

  private class LegacyFixedSizeMutableGlBufferImpl
      : BGlBufferImpl, IFixedSizeMutableGlBuffer {
    private readonly byte[] previousData_;
    
    public override int Id { get; }

    public LegacyFixedSizeMutableGlBufferImpl(
        BufferTarget target,
        int size) : base(target) {
      var id = this.Id = GL.GenBuffer();
      this.previousData_ = new byte[size];

      GL.BindBuffer(target, id);
      GL.BufferData(target,
                    size,
                    this.previousData_,
                    BufferUsageHint.StreamDraw);

      this.previousData_ = new byte[size];
    }

    public void UpdateData(UpdateBuffer updateBuffer) {
      Span<byte> scratchBuffer = stackalloc byte[this.previousData_.Length];
      updateBuffer(scratchBuffer);

      if (scratchBuffer.SequenceEqual(this.previousData_)) {
        return;
      }

      fixed (byte* scratchPtr = &scratchBuffer[0]) {
        GL.BindBuffer(this.Target, this.Id);
        GL.BufferSubData(
            this.Target,
            0,
            scratchBuffer.Length,
            (nint) scratchPtr);
      }
    }
  }
}