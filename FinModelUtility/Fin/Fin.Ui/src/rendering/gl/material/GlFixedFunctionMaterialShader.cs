using System.Numerics;

using fin.language.equations.fixedFunction;
using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.ui.rendering.gl.texture;


namespace fin.ui.rendering.gl.material;

public class GlFixedFunctionMaterialShader(
    IReadOnlyModel model,
    IModelRequirements modelRequirements,
    IReadOnlyFixedFunctionMaterial fixedFunctionMaterial,
    IReadOnlyTextureTransformManager? textureTransformManager)
    : BGlMaterialShader<IReadOnlyFixedFunctionMaterial>(
        model,
        modelRequirements,
        fixedFunctionMaterial,
        textureTransformManager) {
  private IShaderUniform<Vector4>? blendColorUniform_;

  private (IColorRegister, IShaderUniform<Vector3>)[]
      colorRegistersAndUniforms_;

  private (IScalarRegister, IShaderUniform<float>)[]
      scalarRegistersAndUniforms_;

  protected override void DisposeInternal() { }

  protected override void Setup(
      IReadOnlyFixedFunctionMaterial material,
      GlShaderProgram impl) {
    var equations = material.Equations;

    if (equations.DoOutputsDependOn([
            FixedFunctionSource.BLEND_COLOR,
            FixedFunctionSource.BLEND_ALPHA
        ])) {
      this.blendColorUniform_ = impl.GetUniformVec4("blendColor");
    }

    var finTextures = material.TextureSources;
    for (var i = 0; i < MaterialConstants.MAX_TEXTURES; ++i) {
      if (!equations.DoOutputsDependOnTextureSource(i)) {
        continue;
      }

      var finTexture = i < (finTextures?.Count ?? 0)
          ? finTextures[i]
          : null;
      var glTexture = finTexture != null
          ? GlTexture.FromTexture(finTexture)
          : GlMaterialConstants.NULL_WHITE_TEXTURE;

      this.SetUpTexture($"texture{i}", i, finTexture, glTexture);
    }

    var normalTexture = material.NormalTexture;
    if (normalTexture != null) {
      var glTexture = GlTexture.FromTexture(normalTexture);
      this.SetUpTexture("normalTexture",
                        MaterialConstants.MAX_TEXTURES,
                        normalTexture,
                        glTexture);
    }

    var colorRegisterToUniform
        = new Dictionary<IColorRegister, IShaderUniform<Vector3>>();
    var scalarRegisterToUniform
        = new Dictionary<IScalarRegister, IShaderUniform<float>>();

    var registers = material.Registers;
    foreach (var colorRegister in registers.ColorRegisters) {
      colorRegisterToUniform[colorRegister] =
          impl.GetUniformVec3($"color_{colorRegister.Name}");
    }

    foreach (var scalarRegister in registers.ScalarRegisters) {
      scalarRegisterToUniform[scalarRegister] =
          impl.GetUniformFloat($"scalar_{scalarRegister.Name}");
    }

    this.colorRegistersAndUniforms_ = colorRegisterToUniform
                                      .Select(p => (p.Key, p.Value))
                                      .ToArray();
    this.scalarRegistersAndUniforms_ = scalarRegisterToUniform
                                       .Select(p => (p.Key, p.Value))
                                       .ToArray();
  }

  protected override void PassUniformsAndBindTextures(
      GlShaderProgram shaderProgram) {
    if (this.blendColorUniform_ != null) {
      var blendColor = GlUtil.GetBlendColor();
      this.blendColorUniform_.SetAndMarkDirty(
          new Vector4(blendColor.R / 255f,
                      blendColor.G / 255f,
                      blendColor.B / 255f,
                      blendColor.A / 255f));
    }

    foreach (var (register, uniform) in this.colorRegistersAndUniforms_) {
      uniform.SetAndMaybeMarkDirty(new Vector3(register.Value.Rf,
                                               register.Value.Gf,
                                               register.Value.Bf));
    }

    foreach (var (register, uniform) in this.scalarRegistersAndUniforms_) {
      uniform.SetAndMaybeMarkDirty(register.Value);
    }
  }
}