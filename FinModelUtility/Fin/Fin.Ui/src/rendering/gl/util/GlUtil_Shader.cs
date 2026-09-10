using System.Runtime.CompilerServices;
using System.Text;

using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public int CurrentShader { get; set; } = -1;
}

public static partial class GlUtil {
  public const bool VALIDATE_PROGRAM = false;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void UseProgram(int shader) {
    if (currentState_.CurrentShader == shader) {
      return;
    }

    currentState_.CurrentShader = shader;
    GL.UseProgram(shader);
  }

  public static void ValidateCurrentProgram() {
    // Validation is slow, so let's not do it if we don't need to.
    if (!VALIDATE_PROGRAM) {
      return;
    }

    var programId = currentState_.CurrentShader;

    GL.ValidateProgram(programId);
    GL.GetProgram(
        programId,
        GetProgramParameterName.ValidateStatus,
        out var validateStatus);

    if (validateStatus == 0) {
      var errorSb
          = new StringBuilder("Failed to validate shader program: ");

      GL.GetProgram(
          programId,
          GetProgramParameterName.InfoLogLength,
          out var infoLogLength);
      GL.GetProgramInfoLog(
          programId,
          infoLogLength,
          out _,
          out var validateError);
      errorSb.Append(validateError);

      Asserts.Fail(errorSb.ToString());
    }
  }
}