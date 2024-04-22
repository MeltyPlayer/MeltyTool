using System.Numerics;

using fin.math;
using fin.model;


namespace fin.ui.rendering.gl.material {
  public class GlColorMaterialShader(
      IReadOnlyModel model,
      IColorMaterial colorMaterial,
      IBoneTransformManager? boneTransformManager,
      IReadOnlyLighting? lighting)
      : BGlMaterialShader<IColorMaterial>(model,
                                          colorMaterial,
                                          boneTransformManager,
                                          lighting) {
    private readonly IColorMaterial material_ = colorMaterial;
    private IShaderUniform<Vector4> diffuseLightColorUniform_;

    protected override void DisposeInternal() { }

    protected override void Setup(IColorMaterial material,
                                  GlShaderProgram shaderProgram) {
      this.diffuseLightColorUniform_ =
          shaderProgram.GetUniformVec4("diffuseColor");
    }

    protected override void PassUniformsAndBindTextures(GlShaderProgram impl) {
      this.diffuseLightColorUniform_.SetAndMaybeMarkDirty(
          new Vector4(this.material_.Color.R / 255f,
                      this.material_.Color.G / 255f,
                      this.material_.Color.B / 255f,
                      this.material_.Color.A / 255f));
    }
  }
}