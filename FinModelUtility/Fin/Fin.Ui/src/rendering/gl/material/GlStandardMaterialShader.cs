using System.Numerics;

using fin.math;
using fin.model;
using fin.shaders.glsl;


namespace fin.ui.rendering.gl.material;

public sealed class GlStandardMaterialShader(
    IReadOnlyModel model,
    IModelRequirements modelRequirements,
    IReadOnlyStandardMaterial standardMaterial,
    IReadOnlyTextureTransformManager textureTransformManager,
    IReadOnlyTextureSwapManager textureSwapManager)
    : BGlMaterialShader<IReadOnlyStandardMaterial>(
        model,
        modelRequirements,
        standardMaterial,
        textureTransformManager,
        textureSwapManager) {
  private IShaderUniform<Vector4> diffuseColor_;

  protected override void DisposeInternal() { }

  protected override void Setup(IReadOnlyStandardMaterial material,
                                GlShaderProgram shaderProgram) {
    var diffuseFinTexture = material.DiffuseTexture;
    this.SetUpTexture("diffuseTexture",
                      0,
                      diffuseFinTexture,
                      GlMaterialConstants.NULL_WHITE_TEXTURE);

    this.diffuseColor_ = shaderProgram.GetUniformVec4("diffuseColor");

    var normalFinTexture = material.NormalTexture;
    this.SetUpTexture("normalTexture",
                      1,
                      normalFinTexture,
                      GlMaterialConstants.NULL_GRAY_TEXTURE);

    var ambientOcclusionFinTexture = material.AmbientOcclusionTexture;
    this.SetUpTexture("ambientOcclusionTexture",
                      2,
                      ambientOcclusionFinTexture,
                      GlMaterialConstants.NULL_WHITE_TEXTURE);

    var emissiveFinTexture = material.EmissiveTexture;
    this.SetUpTexture("emissiveTexture",
                      3,
                      emissiveFinTexture,
                      GlMaterialConstants.NULL_BLACK_TEXTURE);

    var specularFinTexture = material.SpecularTexture;
    this.SetUpTexture("specularTexture",
                      4,
                      specularFinTexture,
                      GlMaterialConstants.NULL_WHITE_TEXTURE);
  }

  protected override void PassUniformsAndBindTextures(
      GlShaderProgram impl) {
    var diffuseColorVector = standardMaterial.DiffuseColor;
    var diffuseColor = new Vector4(diffuseColorVector?.R ?? 255,
                                   diffuseColorVector?.G ?? 255,
                                   diffuseColorVector?.B ?? 255,
                                   diffuseColorVector?.A ?? 255) /
                       255;

    // TODO: Can we switch this to only be updated sometimes?
    this.diffuseColor_.SetAndMarkDirty(diffuseColor);
  }
}