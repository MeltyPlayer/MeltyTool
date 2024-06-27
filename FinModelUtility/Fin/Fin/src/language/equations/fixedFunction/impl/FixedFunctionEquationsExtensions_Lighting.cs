using fin.model;

namespace fin.language.equations.fixedFunction;

public static partial class FixedFunctionEquationsExtensions {
  public static IColorValue GetMergedLightDiffuseColor(
      this IFixedFunctionEquations<FixedFunctionSource> equations)
    => equations.CreateOrGetColorInput(
        FixedFunctionSource.LIGHT_DIFFUSE_COLOR_MERGED);

  public static IScalarValue GetMergedLightDiffuseAlpha(
      this IFixedFunctionEquations<FixedFunctionSource> equations)
    => equations.CreateOrGetScalarInput(
        FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_MERGED);


  public static IColorValue GetMergedLightSpecularColor(
      this IFixedFunctionEquations<FixedFunctionSource> equations)
    => equations.CreateOrGetColorInput(
        FixedFunctionSource.LIGHT_SPECULAR_COLOR_MERGED);

  public static IScalarValue GetMergedLightSpecularAlpha(
      this IFixedFunctionEquations<FixedFunctionSource> equations)
    => equations.CreateOrGetScalarInput(
        FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED);
}