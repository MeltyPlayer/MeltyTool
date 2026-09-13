using fin.math;
using fin.model;
using fin.shaders.glsl;


namespace fin.ui.rendering.gl.material;

public sealed class GlShaderMaterialShader(
    IReadOnlyModel model,
    IModelRequirements modelRequirements,
    IReadOnlyShaderMaterial shaderMaterial,
    IReadOnlyTextureTransformManager textureTransformManager,
    IReadOnlyTextureSwapManager textureSwapManager)
    : BGlMaterialShader<IReadOnlyShaderMaterial>(
        model,
        modelRequirements,
        shaderMaterial,
        textureTransformManager,
        textureSwapManager) {
  protected override void DisposeInternal() { }

  protected override void Setup(IReadOnlyShaderMaterial material,
                                GlShaderProgram shaderProgram) {
    this.ShaderProgram = shaderProgram;

    var textureIndex = 0;
    foreach (var (uniformName, finTexture) in material.TextureByUniform) {
      this.SetUpTexture(uniformName,
                        textureIndex++,
                        finTexture,
                        GlMaterialConstants.NULL_WHITE_TEXTURE);
    }
  }

  protected override void PassUniformsAndBindTextures(GlShaderProgram impl) { }

  public GlShaderProgram ShaderProgram { get; private set; }
}