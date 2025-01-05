using System.Numerics;

using fin.model;
using fin.shaders.glsl;


namespace fin.ui.rendering.gl.material;

public class GlColorMaterialShader(
    IReadOnlyModel model,
    IModelRequirements modelRequirements,
    IReadOnlyColorMaterial colorMaterial)
    : BGlMaterialShader<IReadOnlyColorMaterial>(
        model,
        modelRequirements,
        colorMaterial,
        null) {
  private IShaderUniform<Vector4> diffuseLightColorUniform_;

  protected override void DisposeInternal() { }

  protected override void Setup(IReadOnlyColorMaterial material,
                                GlShaderProgram shaderProgram) {
    this.diffuseLightColorUniform_ =
        shaderProgram.GetUniformVec4("diffuseColor");
  }

  protected override void PassUniformsAndBindTextures(GlShaderProgram impl) {
    this.diffuseLightColorUniform_.SetAndMarkDirty(
        new Vector4(colorMaterial.Color.R / 255f,
                    colorMaterial.Color.G / 255f,
                    colorMaterial.Color.B / 255f,
                    colorMaterial.Color.A / 255f));
  }
}