using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl.ubo;

public class GlUbo : IDisposable {
  private readonly int bindingIndex_;
  private readonly int id_;
  private readonly byte[] buffer_;

  public GlUbo(int bufferSize, int bindingIndex) {
    this.bindingIndex_ = bindingIndex;
    this.id_ = GL.GenBuffer();
    this.buffer_ = new byte[bufferSize];

    GL.BindBuffer(BufferTarget.UniformBuffer, this.id_);
    GL.BufferData(BufferTarget.UniformBuffer,
                  bufferSize,
                  IntPtr.Zero,
                  BufferUsageHint.StreamDraw);
    GL.BindBuffer(BufferTarget.UniformBuffer, 0);
  }

  ~GlUbo() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => GL.DeleteBuffer(this.id_);

  public unsafe void UpdateDataIfChanged(ReadOnlySpan<byte> newData) {
    if (newData.SequenceEqual(this.buffer_)) {
      return;
    }

    newData.CopyTo(this.buffer_);
    fixed (byte* bufferPtr = &newData.GetPinnableReference()) {
      GL.BindBuffer(BufferTarget.UniformBuffer, this.id_);
      GL.BufferSubData(BufferTarget.UniformBuffer,
                       IntPtr.Zero,
                       new IntPtr(newData.Length),
                       new IntPtr(bufferPtr));
      GL.BindBuffer(BufferTarget.UniformBuffer, 0);
    }
  }

  public void Bind()
    => GL.BindBufferBase(BufferRangeTarget.UniformBuffer,
                         this.bindingIndex_,
                         this.id_);
}