using fin.ui.rendering.gl.buffers;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public IGlBuffer?[] CurrentSsboBufferBaseIdByIndex { get; } = [
      null, null, null, null, null
  ];
}

public static partial class GlUtil {
  public static void ResetSsboBufferBase() {
    for (var i = 0;
         i < currentState_.CurrentSsboBufferBaseIdByIndex.Length;
         ++i) {
      BindSsboBufferBase(i, null);
    }
  }

  public static void BindSsboBufferBase(int bindingIndex, IGlBuffer? buffer) {
    if (currentState_.CurrentSsboBufferBaseIdByIndex[bindingIndex] == buffer) {
      return;
    }

    currentState_.CurrentSsboBufferBaseIdByIndex[bindingIndex] = buffer;
    if (buffer == null) {
      GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer,
                        bindingIndex,
                        0);
    } else {
      GL.BindBufferRange(
          BufferRangeTarget.ShaderStorageBuffer,
          bindingIndex,
          buffer.Id,
          buffer.GpuRangeOffset,
          buffer.GpuRangeLength);
    }
  }
}