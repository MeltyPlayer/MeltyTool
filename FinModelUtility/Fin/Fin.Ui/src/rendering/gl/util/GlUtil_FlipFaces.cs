using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

public partial class GlState {
  public bool FlipFaces { get; set; }
}

public static partial class GlUtil {

  public static void ResetFlipFaces() => SetFlipFaces(false);

  public static void SetFlipFaces(bool flipFaces) {
      if (GlUtil.currentState_.FlipFaces == flipFaces) {
        return;
      }

      GlUtil.currentState_.FlipFaces = flipFaces;
      GL.FrontFace(flipFaces ? FrontFaceDirection.Cw : FrontFaceDirection.Ccw);
    }
}