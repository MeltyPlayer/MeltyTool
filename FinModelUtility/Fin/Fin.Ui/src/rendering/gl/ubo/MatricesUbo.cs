using System.Numerics;

using fin.shaders.glsl;

namespace fin.ui.rendering.gl.ubo;

public class MatricesUbo {
  private readonly int bufferSize_;
  private readonly GlUbo impl_;

  public MatricesUbo(int boneCount) {
    this.bufferSize_ = (3 + (1 + boneCount)) * UboUtil.SIZE_OF_MATRIX4X4;
    this.impl_ = new(this.bufferSize_,
                     GlslConstants.UBO_MATRICES_BINDING_INDEX);
  }

  ~MatricesUbo() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

  public void UpdateData(
      Matrix4x4 modelMatrix,
      Matrix4x4 modelViewMatrix,
      Matrix4x4 projectionMatrix,
      ReadOnlySpan<Matrix4x4> boneMatrices) {
    var offset = 0;
    Span<byte> buffer = stackalloc byte[this.bufferSize_];

    UboUtil.AppendMatrix4x4(buffer, ref offset, modelMatrix);
    UboUtil.AppendMatrix4x4(buffer, ref offset, modelViewMatrix);
    UboUtil.AppendMatrix4x4(buffer, ref offset, projectionMatrix);
    UboUtil.AppendMatrix4x4s(buffer, ref offset, boneMatrices);

    this.impl_.UpdateDataIfChanged(buffer);
  }

  public void Bind() => this.impl_.Bind();
}