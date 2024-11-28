using System.Linq;

using fin.model;

namespace fin.shaders.glsl;

public interface IShaderRequirements {
  public bool UsesSphericalReflectionMapping { get; }
  public bool UsesLinearReflectionMapping { get; }
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
  }

  public bool UsesSphericalReflectionMapping { get; }
  public bool UsesLinearReflectionMapping { get; }
}