using fin.math;
using fin.model;
using fin.shaders.glsl;

namespace fin.ui.rendering.gl.material;

public class GlHiddenMaterialShader(
    IReadOnlyModel model,
    IReadOnlyBoneTransformManager? boneTransformManager,
    IReadOnlyLighting? lighting)
    : BGlMaterialShader<IReadOnlyMaterial?>(
        model,
        null,
        boneTransformManager,
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