using fin.math;
using fin.model;

namespace fin.ui.rendering.gl.material {
  public class GlStandardMaterialShader(
      IReadOnlyModel model,
      IReadOnlyStandardMaterial standardMaterial,
      IReadOnlyBoneTransformManager? boneTransformManager,
      IReadOnlyLighting? lighting)
      : BGlMaterialShader<IReadOnlyStandardMaterial>(model,
        standardMaterial,
        boneTransformManager,
        lighting) {
    protected override void DisposeInternal() { }

    protected override void Setup(IReadOnlyStandardMaterial material,
                                  GlShaderProgram shaderProgram) {
      var diffuseFinTexture = material.DiffuseTexture;
      var diffuseGlTexture =
          diffuseFinTexture != null
              ? GlTexture.FromTexture(diffuseFinTexture)
              : GlMaterialConstants.NULL_WHITE_TEXTURE;
      this.SetUpTexture("diffuseTexture",
                        0,
                        diffuseFinTexture,
                        diffuseGlTexture);

      var normalFinTexture = material.NormalTexture;
      var normalGlTexture = normalFinTexture != null
          ? GlTexture.FromTexture(normalFinTexture)
          : GlMaterialConstants.NULL_GRAY_TEXTURE;
      this.SetUpTexture("normalTexture",
                        1,
                        normalFinTexture,
                        normalGlTexture);

      var ambientOcclusionFinTexture = material.AmbientOcclusionTexture;
      var ambientOcclusionGlTexture = ambientOcclusionFinTexture != null
          ? GlTexture.FromTexture(ambientOcclusionFinTexture)
          : GlMaterialConstants.NULL_WHITE_TEXTURE;
      this.SetUpTexture("ambientOcclusionTexture",
                        2,
                        ambientOcclusionFinTexture,
                        ambientOcclusionGlTexture);

      var emissiveFinTexture = material.EmissiveTexture;
      var emissiveGlTexture = emissiveFinTexture != null
          ? GlTexture.FromTexture(emissiveFinTexture)
          : GlMaterialConstants.NULL_BLACK_TEXTURE;
      this.SetUpTexture("emissiveTexture",
                        3,
                        emissiveFinTexture,
                        emissiveGlTexture);

      var specularFinTexture = material.SpecularTexture;
      var specularGlTexture = specularFinTexture != null
          ? GlTexture.FromTexture(specularFinTexture)
          : GlMaterialConstants.NULL_WHITE_TEXTURE;
      this.SetUpTexture("specularTexture",
                        4,
                        specularFinTexture,
                        specularGlTexture);
    }

    protected override void PassUniformsAndBindTextures(
        GlShaderProgram impl) { }
  }
}