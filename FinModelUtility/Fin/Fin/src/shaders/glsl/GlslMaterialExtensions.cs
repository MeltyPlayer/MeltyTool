using System;

using fin.model;
using fin.shaders.glsl.source;
using fin.util.asserts;

namespace fin.shaders.glsl;

public static class GlslMaterialExtensions {
  public static IShaderSourceGlsl ToShaderSource(
      this IReadOnlyMaterial? material,
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      bool isSelectable) {
    var shaderRequirements
        = ShaderRequirements.FromModelAndMaterial(model,
                                                  modelRequirements,
                                                  material);

    return material.GetShaderType() switch {
        FinShaderType.FIXED_FUNCTION
            => new FixedFunctionShaderSourceGlsl(
                model,
                modelRequirements,
                Asserts.AsA<IReadOnlyFixedFunctionMaterial>(material),
                shaderRequirements,
                isSelectable),
        FinShaderType.TEXTURE => new TextureShaderSourceGlsl(
            model,
            modelRequirements,
            Asserts.AsA<IReadOnlyTextureMaterial>(material),
            shaderRequirements,
            isSelectable),
        FinShaderType.COLOR => new ColorShaderSourceGlsl(
            model,
            modelRequirements,
            Asserts.AsA<IReadOnlyColorMaterial>(material),
            shaderRequirements,
            isSelectable),
        FinShaderType.SHADER => new ShaderShaderSourceGlsl(
            Asserts.AsA<IReadOnlyShaderMaterial>(material)),
        FinShaderType.STANDARD => new StandardShaderSourceGlsl(
            model,
            modelRequirements,
            Asserts.AsA<IReadOnlyStandardMaterial>(material),
            shaderRequirements,
            isSelectable),
        FinShaderType.HIDDEN => new HiddenShaderSourceGlsl(),
        FinShaderType.NULL => new NullShaderSourceGlsl(model,
          modelRequirements,
          shaderRequirements,
          isSelectable),
        _ => throw new ArgumentOutOfRangeException()
    };
  }
}