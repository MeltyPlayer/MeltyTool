using fin.model;
using fin.shaders.glsl;

namespace fin.ui.rendering.gl.material;

public class GlHiddenMaterialShader(
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
    => new HiddenShaderSourceGlsl();

  protected override void Setup(IReadOnlyMaterial? material,
                                GlShaderProgram shaderProgram) { }

  protected override void PassUniformsAndBindTextures(
      GlShaderProgram shaderProgram) { }
}