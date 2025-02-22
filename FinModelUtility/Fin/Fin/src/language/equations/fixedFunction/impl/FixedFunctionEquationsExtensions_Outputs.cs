using fin.model;

namespace fin.language.equations.fixedFunction;

public static partial class FixedFunctionEquationsExtensions {
  public static void SetOutputColorAlpha(
      this IFixedFunctionEquations<FixedFunctionSource> equations,
      (IColorValue? color, IScalarValue? alpha) output) {
    equations.CreateColorOutput(FixedFunctionSource.OUTPUT_COLOR,
                                output.color ?? equations.ColorOps.Zero);
    equations.CreateScalarOutput(FixedFunctionSource.OUTPUT_ALPHA,
                                 output.alpha ?? equations.ScalarOps.Zero);
  }

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