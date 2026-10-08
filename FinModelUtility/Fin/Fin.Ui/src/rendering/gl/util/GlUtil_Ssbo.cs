using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public int CurrentSsboDataId { get; set; }
  public int[] CurrentSsboBufferBaseIdByIndex { get; } = [0, 0, 0, 0, 0];
}

public static partial class GlUtil {
  public static void ResetSsboData() => BindSsboData(0);

  public static void BindSsboData(int ssboId) {
    if (currentState_.CurrentUboDataId == ssboId) {
      return;
    }

    GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ssboId);
  }

  public static void ResetSsboBufferBase() {
    for (var i = 0;
         i < currentState_.CurrentSsboBufferBaseIdByIndex.Length;
         ++i) {
      BindSsboBufferBase(i, 0);
    }
  }

  public static void BindSsboBufferBase(int bindingIndex, int ssboId) {
    if (currentState_.CurrentSsboBufferBaseIdByIndex[bindingIndex] == ssboId) {
      return;
    }

    currentState_.CurrentSsboBufferBaseIdByIndex[bindingIndex] = ssboId;
    GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer,
                      bindingIndex,
                      ssboId);
  }
}