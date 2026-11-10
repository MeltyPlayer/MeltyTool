using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl.buffers;

public static partial class GlBuffer {
  private sealed class LegacyStaticGlBufferImpl : BGlBufferImpl {
    public override int Id { get; }

    public override int GpuRangeOffset { get; }
    public override int GpuRangeLength { get; }

    public unsafe LegacyStaticGlBufferImpl(BufferTarget target,
                                           ReadOnlySpan<byte> data) : base(target) {
      this.GpuRangeOffset = 0;
      this.GpuRangeLength = data.Length;

      fixed (byte* ptr = &data[0]) {
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