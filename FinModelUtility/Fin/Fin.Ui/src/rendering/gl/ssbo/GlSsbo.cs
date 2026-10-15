using fin.ui.rendering.gl.buffers;
using fin.util.spans;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl.ssbo;

public sealed class GlSsbo : IDisposable {
  private readonly int bindingIndex_;
  private readonly IFixedSizeMutableGlBuffer impl_;

  public GlSsbo(int bufferSize, int bindingIndex) {
    this.bindingIndex_ = bindingIndex;
    this.impl_ = GlBuffer.CreateFixedSizeMutableBuffer(
        BufferTarget.ShaderStorageBuffer,
        bufferSize);
  }

  ~GlSsbo() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

  public WriteOnlySpan<byte> Buffer => this.impl_.Buffer;
  public void Flush() => this.impl_.Flush();

  public void Bind()
    => GlUtil.BindSsboBufferBase(this.bindingIndex_, this.impl_.Id);
}