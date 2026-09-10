using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public int CurrentUboDataId { get; set; }
  public int[] CurrentUboBufferBaseIdByIndex { get; } = [0, 0, 0, 0];
}

public static partial class GlUtil {
  public static void ResetUboData() => BindUboData(0);

  public static void BindUboData(int uboId) {
    if (currentState_.CurrentUboDataId == uboId) {
      return;
    }

    GL.BindBuffer(BufferTarget.UniformBuffer, uboId);
  }

  public static void ResetUboBufferBase() {
    BindUboBufferBase(0, 0);
    BindUboBufferBase(1, 0);
    BindUboBufferBase(2, 0);
    BindUboBufferBase(3, 0);
  }

  public static void BindUboBufferBase(int bindingIndex, int uboId) {
    if (currentState_.CurrentUboBufferBaseIdByIndex[bindingIndex] == uboId) {
      return;
    }

    currentState_.CurrentUboBufferBaseIdByIndex[bindingIndex] = uboId;
    GL.BindBufferBase(BufferRangeTarget.UniformBuffer,
                      bindingIndex,
                      uboId);
  }
}