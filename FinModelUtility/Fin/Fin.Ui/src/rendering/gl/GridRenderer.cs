using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl {
  public class GridRenderer {
    public float Spacing { get; } = 32;
    public float Size = 1024;

    public void Render() {
      GlTransform.PassMatricesIntoGl();

      GlUtil.ResetDepth();

      var size = this.Size;
      var spacing = this.Spacing;

      GL.LineWidth(1);

      GL.Begin(PrimitiveType.Lines);

      for (var y = 0f; y <= size / 2; y += spacing) {
        if (y == 0) {
          GL.Color3(1f, 0, 0);
        } else {
          GL.Color3(1f, 1, 1);

          GL.Vertex3(-size / 2, -y, 0);
          GL.Vertex3(size / 2, -y, 0);
        }

        GL.Vertex3(-size / 2, y, 0);
        GL.Vertex3(size / 2, y, 0);
      }

      for (var x = 0f; x <= size / 2; x += spacing) {
        if (x == 0) {
          GL.Color3(0f, 1, 0);
        } else {
          GL.Color3(1f, 1, 1);
          
          GL.Vertex3(-x, -size / 2, 0);
          GL.Vertex3(-x, size / 2, 0);
        }

        GL.Vertex3(x, -size / 2, 0);
        GL.Vertex3(x, size / 2, 0);
      }

      GL.End();
    }
  }
}