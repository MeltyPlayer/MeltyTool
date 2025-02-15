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
                                GlShaderProgram shaderProgram) {
    this.ShaderProgram = shaderProgram;

    var textureIndex = 0;
    foreach (var (uniformName, finTexture) in material.TextureByUniform) {
      var glTexture = GlTexture.FromTexture(finTexture);
      this.SetUpTexture(uniformName, textureIndex++, finTexture, glTexture);
    }
  }

  protected override void PassUniformsAndBindTextures(GlShaderProgram impl) { }

  public GlShaderProgram ShaderProgram { get; private set; }
}