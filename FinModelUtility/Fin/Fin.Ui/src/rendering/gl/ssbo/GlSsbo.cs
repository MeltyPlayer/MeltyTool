using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl.ssbo;

public sealed class GlSsbo : IDisposable {
  private readonly int bindingIndex_;
  private readonly int id_;
  private readonly byte[] buffer_;

  public GlSsbo(int bufferSize, int bindingIndex) {
    this.bindingIndex_ = bindingIndex;
    this.id_ = GL.GenBuffer();
    this.buffer_ = new byte[bufferSize];

    GlUtil.BindSsboData(this.id_);
    GL.BufferData(BufferTarget.ShaderStorageBuffer,
                  bufferSize,
                  IntPtr.Zero,
                  BufferUsageHint.StreamDraw);
    GlUtil.ResetSsboData();
  }

  ~GlSsbo() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => GL.DeleteBuffer(this.id_);

  public void UpdateDataIfChanged(ReadOnlySpan<byte> newData) {
    if (newData.SequenceEqual(this.buffer_)) {
      return;
    }

    this.UpdateData(newData);
  }

  public unsafe void UpdateData(ReadOnlySpan<byte> newData) {
    newData.CopyTo(this.buffer_);
    fixed (byte* bufferPtr = &newData.GetPinnableReference()) {
      GlUtil.BindSsboData(this.id_);
      GL.BufferSubData(BufferTarget.ShaderStorageBuffer,
                       IntPtr.Zero,
                       new IntPtr(newData.Length),
                       new IntPtr(bufferPtr));
      GlUtil.ResetSsboData();
    }
  }

  public void Bind() => GlUtil.BindSsboBufferBase(this.bindingIndex_, this.id_);
}