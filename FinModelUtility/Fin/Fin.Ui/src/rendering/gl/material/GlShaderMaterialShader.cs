using fin.model;
using fin.shaders.glsl;


namespace fin.ui.rendering.gl.material;

public class GlShaderMaterialShader(
    IReadOnlyModel model,
    IModelRequirements modelRequirements,
    IReadOnlyShaderMaterial shaderMaterial)
    : BGlMaterialShader<IReadOnlyShaderMaterial>(
        model,
        modelRequirements,
        shaderMaterial,
        null) {
  protected override void DisposeInternal() { }

  protected override void Setup(IReadOnlyShaderMaterial material,
                                GlShaderProgram shaderProgram) { }

  protected override void PassUniformsAndBindTextures(GlShaderProgram impl) { }
}