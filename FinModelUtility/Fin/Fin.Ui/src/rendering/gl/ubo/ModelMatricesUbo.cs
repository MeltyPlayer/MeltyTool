using System.Numerics;

using fin.shaders.glsl;
using fin.ui.rendering.gl.ubo;

namespace fin.ui.rendering.gl;

public static partial class ModelMatricesBo {
  private sealed class ModelMatricesUbo : IModelMatricesBo {
    private readonly int bufferSize_;
    private readonly GlUbo impl_;

    public ModelMatricesUbo(int boneCount) {
      this.bufferSize_ = (1 + (1 + boneCount)) * UboUtil.SIZE_OF_MATRIX4X4;
      this.impl_ = new(this.bufferSize_,
                       GlslConstants.UBO_CURRENT_MATRICES_BINDING_INDEX);
    }

    ~ModelMatricesUbo() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

    public void UpdateData(in Matrix4x4 modelMatrix,
                           ReadOnlySpan<Matrix4x4> boneMatrices) {
      var offset = 0;
      Span<byte> buffer = stackalloc byte[this.bufferSize_];

      // TODO: Merge model/view/projection matrices with bone matrices here
      // rather than in the shader
      UboUtil.AppendMatrix4x4(buffer, ref offset, in modelMatrix);
      UboUtil.AppendMatrix4x4s(buffer, ref offset, boneMatrices);

      this.impl_.UpdateDataIfChanged(buffer);
    }

    public void Bind() => this.impl_.Bind();
  }
}