using System.Numerics;

using fin.math;
using fin.model;
using fin.model.util;

namespace fin.ui.rendering.gl.material;

public class GlTextureMaterialShader(
    IReadOnlyModel model,
    IReadOnlyTextureMaterial material,
    IReadOnlyTextureTransformManager? textureTransformManager)
    : BGlMaterialShader<IReadOnlyTextureMaterial>(
        model,
        material,
        textureTransformManager) {
  private IShaderUniform<Vector4> diffuseColorUniform_;

  protected override void DisposeInternal() { }

  protected override void Setup(
      IReadOnlyTextureMaterial material,
      GlShaderProgram shaderProgram) {
    var finTexture = PrimaryTextureFinder.GetFor(material);
    var glTexture = finTexture != null
        ? GlTexture.FromTexture(finTexture)
        : GlMaterialConstants.NULL_WHITE_TEXTURE;

    this.SetUpTexture("diffuseTexture", 0, finTexture, glTexture);

    this.diffuseColorUniform_ = shaderProgram.GetUniformVec4("diffuseColor");
  }

  protected override void PassUniformsAndBindTextures(
      GlShaderProgram shaderProgram) {
    var diffuseColor = material.DiffuseColor;
    if (diffuseColor != null) {
      this.diffuseColorUniform_.SetAndMarkDirty(
          new Vector4(diffuseColor.Value.R / 255f,
                      diffuseColor.Value.G / 255f,
                      diffuseColor.Value.B / 255f,
                      diffuseColor.Value.A / 255f));
    }
  }
}