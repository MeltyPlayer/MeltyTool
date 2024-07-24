using System.Numerics;

using fin.shaders.glsl;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl.ubo;

public class MatricesUbo {
  private readonly int sizeOfBuffer_;
  private readonly int id_;

  public MatricesUbo(int boneCount) {
    this.sizeOfBuffer_ = (3 + (1 + boneCount)) * UboUtil.SIZE_OF_MATRIX4X4;
    this.id_ = GL.GenBuffer();

    GL.BindBuffer(BufferTarget.UniformBuffer, this.id_);
    GL.BufferData(BufferTarget.UniformBuffer,
                  this.sizeOfBuffer_,
                  IntPtr.Zero,
                  BufferUsageHint.StreamDraw);
    GL.BindBuffer(BufferTarget.UniformBuffer, 0);
  }

  public unsafe void UpdateData(
      Matrix4x4 modelMatrix,
      Matrix4x4 modelViewMatrix,
      Matrix4x4 projectionMatrix,
      ReadOnlySpan<Matrix4x4> boneMatrices) {
    var offset = 0;
    Span<byte> buffer = stackalloc byte[this.sizeOfBuffer_];

    UboUtil.AppendMatrix4x4(buffer, ref offset, modelMatrix);
    UboUtil.AppendMatrix4x4(buffer, ref offset, modelViewMatrix);
    UboUtil.AppendMatrix4x4(buffer, ref offset, projectionMatrix);
    UboUtil.AppendMatrix4x4s(buffer, ref offset, boneMatrices);

    fixed (byte* bufferPtr = &buffer.GetPinnableReference()) {
      GL.BindBuffer(BufferTarget.UniformBuffer, this.id_);
      GL.BufferSubData(BufferTarget.UniformBuffer,
                       IntPtr.Zero,
                       new IntPtr(buffer.Length),
                       new IntPtr(bufferPtr));
      GL.BindBuffer(BufferTarget.UniformBuffer, 0);
    }
  }

  public void Bind()
    => GL.BindBufferBase(BufferRangeTarget.UniformBuffer,
                         GlslConstants.UBO_MATRICES_BINDING_INDEX,
                         this.id_);
}