using System.Linq;

using fin.language.equations.fixedFunction;
using fin.model;
using fin.util.asserts;

namespace fin.shaders.glsl;

public interface IShaderRequirements {
  public bool UsesSphericalReflectionMapping { get; }
  public bool UsesLinearReflectionMapping { get; }

  public bool[] UsedUvs { get; }
  public bool[] UsedColors { get; }
}

public class ShaderRequirements : IShaderRequirements {
  public static ShaderRequirements FromModelAndMaterial(IReadOnlyModel model,
    IReadOnlyMaterial? material)
    => new(model, material);

  private ShaderRequirements(IReadOnlyModel model,
                             IReadOnlyMaterial? material) {
    var modelRequirements = ModelRequirements.FromModel(model);

    this.UsesSphericalReflectionMapping
        = material?.Textures.Any(t => t.UvType is UvType.SPHERICAL) ?? false;
    this.UsesLinearReflectionMapping
        = material?.Textures.Any(t => t.UvType is UvType.LINEAR) ?? false;

    this.UsedUvs = new bool[MaterialConstants.MAX_UVS];
    if (material != null && material is not IFixedFunctionMaterial) {
      foreach (var texture in material.Textures) {
        var uvIndex = texture.UvIndex;
        Asserts.True(modelRequirements.NumUvs >= uvIndex + 1);
        this.UsedUvs[uvIndex] = true;
      }
    }

    this.UsedColors = new bool[MaterialConstants.MAX_COLORS];
    switch (material) {
      case IColorMaterial
           or INullMaterial
           or ITextureMaterial
           or IStandardMaterial
           or null: {
        this.UsedColors[0] = modelRequirements.NumColors > 0;
        break;
      }
      case IFixedFunctionMaterial fixedFunctionMaterial: {
        var equations = fixedFunctionMaterial.Equations;
        for (var i = 0; i < fixedFunctionMaterial.TextureSources.Count; ++i) {
          var textureSource = fixedFunctionMaterial.TextureSources[i];
          if (textureSource == null) {
            continue;
          }

          if (equations.DoOutputsDependOnTextureSource(i)) {
            if (textureSource.UvType == UvType.STANDARD) {
              var uvIndex = textureSource.UvIndex;
              Asserts.True(modelRequirements.NumUvs >= uvIndex + 1);
              this.UsedUvs[uvIndex] = true;
            }
          }
        }

        for (var i = 0; i < this.UsedColors.Length; ++i) {
          if (equations.DoOutputsDependOn([
                  FixedFunctionSource.VERTEX_COLOR_0 + i,
                  FixedFunctionSource.VERTEX_ALPHA_0 + i
              ])) {
            Asserts.True(modelRequirements.NumColors >= i + 1);
            this.UsedColors[i] = true;
          }
        }

        break;
      }
    }
  }

  public bool UsesSphericalReflectionMapping { get; }
  public bool UsesLinearReflectionMapping { get; }

  public bool[] UsedUvs { get; }
  public bool[] UsedColors { get; }
}