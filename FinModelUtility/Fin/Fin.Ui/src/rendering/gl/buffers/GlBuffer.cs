using OpenTK.Graphics.OpenGL4;

using readOnly;

namespace fin.ui.rendering.gl.buffers;

[GenerateReadOnly]
public partial interface IGlBuffer : IDisposable {
  int Id { get; }
  BufferTarget Target { get; }
}

public static class GlBuffer {
  public static IGlBuffer CreateStaticBuffer(
      BufferTarget target,
      ReadOnlySpan<byte> data)
    => new StaticGlBuffer(target, data);

  private class StaticGlBuffer : IGlBuffer {
    public int Id { get; }
    public BufferTarget Target { get; }

    public unsafe StaticGlBuffer(BufferTarget target, ReadOnlySpan<byte> data) {
      this.Target = target;

      this.Id = GL.GenBuffer();

      fixed (byte* ptr = &data[0]) {
        if (OpenGlFeatureService.SupportsBufferStorage) {
          GL.BindBuffer(target, this.Id);
          GL.BufferStorage(target,
                           data.Length,
                           (nint) ptr,
                           BufferStorageFlags.None);
        } else {
          GL.BindBuffer(target, this.Id);
          GL.BufferData(target,
                        data.Length,
                        (nint) ptr,
                        BufferUsageHint.StaticDraw);
        }
      }
    }

    ~StaticGlBuffer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() => GL.DeleteBuffer(this.Id);
  }
}