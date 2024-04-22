using fin.math;
using fin.model;
using fin.model.util;

namespace fin.ui.rendering.gl.material {
  public class GlTextureMaterialShader(
      IReadOnlyModel model,
      IReadOnlyMaterial material,
      IBoneTransformManager? boneTransformManager,
      IReadOnlyLighting? lighting)
      : BGlMaterialShader<IReadOnlyMaterial>(model,
                                             material,
                                             boneTransformManager,
                                             lighting) {
    protected override void DisposeInternal() { }

    protected override void Setup(
        IReadOnlyMaterial material,
        GlShaderProgram shaderProgram) {
      var finTexture = PrimaryTextureFinder.GetFor(material);
      var glTexture = finTexture != null
          ? GlTexture.FromTexture(finTexture)
          : GlMaterialConstants.NULL_WHITE_TEXTURE;

      this.SetUpTexture("diffuseTexture", 0, finTexture, glTexture);
    }

    protected override void PassUniformsAndBindTextures(
        GlShaderProgram shaderProgram) { }
  }
}