using System.Linq;

using fin.model;

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
    this.UsesSphericalReflectionMapping
        = material?.Textures.Any(t => t.UvType is UvType.SPHERICAL) ?? false;
    this.UsesLinearReflectionMapping
        = material?.Textures.Any(t => t.UvType is UvType.LINEAR) ?? false;

    this.UsedUvs = new bool[MaterialConstants.MAX_UVS];
    if (material != null) {
      foreach (var texture in material.Textures) {
        this.UsedUvs[texture.UvIndex] = true;
      }
    }

    this.UsedColors = new bool[MaterialConstants.MAX_COLORS];
    switch (material) {
      case IColorMaterial
           or INullMaterial
           or ITextureMaterial
           or IStandardMaterial
           or null: {
        this.UsedColors[0] = true;
        break;
      }
      case IFixedFunctionMaterial fixedFunctionMaterial: {
        var equations = fixedFunctionMaterial.Equations;
        for (var i = 0; i < this.UsedColors.Length; ++i) {
          this.UsedColors[i] = equations.DoOutputsDependOn([
              FixedFunctionSource.VERTEX_COLOR_0 + i,
              FixedFunctionSource.VERTEX_ALPHA_0 + i
          ]);
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