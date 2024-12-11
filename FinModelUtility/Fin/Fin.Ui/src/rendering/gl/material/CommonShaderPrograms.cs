using fin.shaders.glsl;

namespace fin.ui.rendering.gl.material;

public static class CommonShaderPrograms {
  private static GlShaderProgram? texturelessShaderProgram_;

  public static GlShaderProgram TEXTURELESS_SHADER_PROGRAM {
    get {
      if (texturelessShaderProgram_ == null) {
        texturelessShaderProgram_ =
            GlShaderProgram.FromShaders(
                """
                #version 120

                varying vec4 vertexColor;

                void main() {
                    gl_Position = gl_ProjectionMatrix * gl_ModelViewMatrix * gl_Vertex; 
                    vertexColor = gl_Color;
                }
                """,
                $$"""
                #version 130
                {{GlslConstants.FLOAT_PRECISION}}

                out vec4 fragColor;

                in vec4 vertexColor;

                void main() {
                    fragColor = vertexColor;
                }
                """);
      }

      return texturelessShaderProgram_;
    }
  }
}