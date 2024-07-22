using fin.model;
using fin.shaders.glsl;

namespace fin.ui.rendering.gl.material;

public class GlNullMaterialShader(
    IReadOnlyModel model,
    IReadOnlyLighting? lighting)
    : BGlMaterialShader<IReadOnlyMaterial?>(
        model,
        null,
        null,
        lighting) {
  protected override void DisposeInternal() { }

  protected override IShaderSourceGlsl GenerateShaderSource(
      IReadOnlyModel model,
      IReadOnlyMaterial? material)
    => new NullShaderSourceGlsl(model, true);

  protected override void Setup(IReadOnlyMaterial? material,
                                GlShaderProgram shaderProgram) { }

  protected override void PassUniformsAndBindTextures(
      GlShaderProgram shaderProgram) { }
}