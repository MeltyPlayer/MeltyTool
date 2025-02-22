using fin.model;
using fin.util.enumerables;

namespace fin.language.equations.fixedFunction;

public static partial class FixedFunctionEquationsExtensions {
  public static int AddTextureSource(this IFixedFunctionMaterial material,
                                     IReadOnlyTexture texture) {
    var index = material.TextureSources.IndexOf(t => t == null);
    material.SetTextureSource(index, texture);
    return index;
  }

  public static IColorValue AddTextureSourceColor(
      this IFixedFunctionMaterial material,
      IReadOnlyTexture texture) {
    var index = material.AddTextureSource(texture);

    var equations = material.Equations;
    var color = equations.CreateOrGetColorInput(
        FixedFunctionSource.TEXTURE_COLOR_0 +
        index);

    return color;
  }

  public static (IColorValue, IScalarValue) AddTextureSourceColorAlpha(
      this IFixedFunctionMaterial material,
      IReadOnlyTexture texture) {
    var index = material.AddTextureSource(texture);

    var equations = material.Equations;
    var color = equations.CreateOrGetColorInput(
        FixedFunctionSource.TEXTURE_COLOR_0 +
        index);
    var alpha = equations.CreateOrGetScalarInput(
        FixedFunctionSource.TEXTURE_ALPHA_0 +
        index);

    return (color, alpha);
  }

  public static bool DoOutputsDependOnTextureSource(
      this IFixedFunctionEquations<FixedFunctionSource> impl,
      int i) => impl.DoOutputsDependOn(
  [
      FixedFunctionSource.TEXTURE_COLOR_0 + i,
      FixedFunctionSource.TEXTURE_ALPHA_0 + i
  ]);
}