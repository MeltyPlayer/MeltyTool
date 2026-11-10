using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial record GlState {
  public bool FlipFaces { get; set; }
}

public static partial class GlUtil {

  public static void ResetFlipFaces() => SetFlipFaces(false);

  public static void SetFlipFaces(bool flipFaces) {
      if (currentState_.FlipFaces == flipFaces) {
        return;
      }

      currentState_.FlipFaces = flipFaces;
      GL.FrontFace(flipFaces ? FrontFaceDirection.Cw : FrontFaceDirection.Ccw);
    }
}