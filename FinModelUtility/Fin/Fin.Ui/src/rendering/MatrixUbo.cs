using System.Numerics;

using CommunityToolkit.HighPerformance;

using fin.shaders.glsl;
using fin.ui.rendering.gl;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering;

public class MatrixUbo {
  private const int SIZE_OF_MATRIX = 4 * 4 * 4;

  private readonly int sizeOfBuffer_;
  private readonly int id_;

  public MatrixUbo(int boneCount) {
    this.sizeOfBuffer_ = (3 + (1 + boneCount)) * SIZE_OF_MATRIX;
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

    buffer.Slice(offset, SIZE_OF_MATRIX).Cast<byte, Matrix4x4>()[0]
        = modelMatrix;
    offset += SIZE_OF_MATRIX;

    buffer.Slice(offset, SIZE_OF_MATRIX).Cast<byte, Matrix4x4>()[0]
        = modelViewMatrix;
    offset += SIZE_OF_MATRIX;

    buffer.Slice(offset, SIZE_OF_MATRIX).Cast<byte, Matrix4x4>()[0]
        = projectionMatrix;
    offset += SIZE_OF_MATRIX;

    boneMatrices.CopyTo(
        buffer.Slice(offset, boneMatrices.Length * SIZE_OF_MATRIX)
              .Cast<byte, Matrix4x4>());
    offset += boneMatrices.Length * SIZE_OF_MATRIX;

    fixed (byte* bufferPtr = &buffer.GetPinnableReference()) {
      GL.BindBuffer(BufferTarget.UniformBuffer, this.id_);
      GL.BufferSubData(BufferTarget.UniformBuffer,
                       IntPtr.Zero,
                       new IntPtr(buffer.Length),
                       new IntPtr(bufferPtr));
      GL.BindBuffer(BufferTarget.UniformBuffer, 0);
    }
  }

  public void Bind() {
    GL.BindBufferBase(BufferRangeTarget.UniformBuffer,
                      GlslConstants.UBO_MATRIX_BINDING_INDEX,
                      this.id_);
  }
}