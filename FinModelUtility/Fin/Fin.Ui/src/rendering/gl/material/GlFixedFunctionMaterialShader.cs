using System.Numerics;

using fin.language.equations.fixedFunction;
using fin.math;
using fin.model;


namespace fin.ui.rendering.gl.material {
  public class GlFixedFunctionMaterialShader(
      IModel model,
      IReadOnlyFixedFunctionMaterial fixedFunctionMaterial,
      IBoneTransformManager? boneTransformManager,
      IReadOnlyLighting? lighting)
      : BGlMaterialShader<IReadOnlyFixedFunctionMaterial>(
          model,
          fixedFunctionMaterial,
          boneTransformManager,
          lighting) {
    private readonly Dictionary<IColorRegister, IShaderUniform<Vector3>>
        colorRegistersAndUniforms_ = new();

    private readonly Dictionary<IScalarRegister, IShaderUniform<float>>
        scalarRegistersAndUniforms_ = new();

    protected override void DisposeInternal() { }

    protected override void Setup(
        IReadOnlyFixedFunctionMaterial material,
        GlShaderProgram impl) {
      var finTextures = material.TextureSources;

      var equations = material.Equations;
      for (var i = 0; i < MaterialConstants.MAX_TEXTURES; ++i) {
        if (!equations.DoOutputsDependOn(
            [
                FixedFunctionSource.TEXTURE_COLOR_0 + i,
                FixedFunctionSource.TEXTURE_ALPHA_0 + i
            ])) {
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

      var registers = material.Registers;
      foreach (var colorRegister in registers.ColorRegisters) {
        this.colorRegistersAndUniforms_[colorRegister] =
            impl.GetUniformVec3($"color_{colorRegister.Name}");
      }

      foreach (var scalarRegister in registers.ScalarRegisters) {
        this.scalarRegistersAndUniforms_[scalarRegister] =
            impl.GetUniformFloat($"scalar_{scalarRegister.Name}");
      }
    }

    protected override void PassUniformsAndBindTextures(
        GlShaderProgram shaderProgram) {
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
}