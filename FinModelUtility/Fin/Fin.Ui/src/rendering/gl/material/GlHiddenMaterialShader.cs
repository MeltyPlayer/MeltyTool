using fin.model;
using fin.shaders.glsl;
using fin.shaders.glsl.source;

namespace fin.ui.rendering.gl.material;

public class GlHiddenMaterialShader(
    IReadOnlyModel model,
    IModelRequirements modelRequirements)
    : BGlMaterialShader<IReadOnlyMaterial?>(model,
                                            modelRequirements,
                                            null,
                                            null) {
  protected override void DisposeInternal() { }

  protected override IShaderSourceGlsl GenerateShaderSource(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      IReadOnlyMaterial? material)
    => new HiddenShaderSourceGlsl();

  protected override void Setup(IReadOnlyMaterial? material,
                                GlShaderProgram shaderProgram) { }

  protected override void PassUniformsAndBindTextures(
      GlShaderProgram shaderProgram) { }
}