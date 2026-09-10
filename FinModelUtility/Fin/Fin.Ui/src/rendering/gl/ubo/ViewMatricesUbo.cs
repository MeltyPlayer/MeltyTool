using fin.shaders.glsl;

namespace fin.ui.rendering.gl.ubo;

public sealed class ViewMatricesUbo : IDisposable {
  private readonly int bufferSize_;
  private readonly GlUbo impl_;

  public ViewMatricesUbo() {
    this.bufferSize_ = UboUtil.SIZE_OF_MATRIX4X4;
    this.impl_ = new(this.bufferSize_,
                     GlslConstants.UBO_GLOBAL_MATRICES_BINDING_INDEX);
  }

  ~ViewMatricesUbo() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

  public void UpdateData() {
    var offset = 0;
    Span<byte> buffer = stackalloc byte[this.bufferSize_];

    // TODO: Pass in normal matrices here
    UboUtil.AppendMatrix4x4(buffer,
                            ref offset,
                            GlTransform.ViewMatrix *
                            GlTransform.ProjectionMatrix);

    this.impl_.UpdateDataIfChanged(buffer);
  }

  public void Bind() => this.impl_.Bind();
}