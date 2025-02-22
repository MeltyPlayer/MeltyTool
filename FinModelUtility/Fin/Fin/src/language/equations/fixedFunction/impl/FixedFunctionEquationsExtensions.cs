using fin.model;

namespace fin.language.equations.fixedFunction;

public static partial class FixedFunctionEquationsExtensions {
  public static (IColorValue?, IScalarValue?) GenerateLighting(
      this IFixedFunctionEquations<FixedFunctionSource> equations,
      (IColorValue? color, IScalarValue? alpha) diffuse,
      IColorValue? ambient)
    => GenerateLighting(equations, diffuse, ambient, equations.ColorOps.Zero);

  public static (IColorValue?, IScalarValue?) GenerateLighting(
      this IFixedFunctionEquations<FixedFunctionSource> equations,
      (IColorValue? color, IScalarValue? alpha) diffuse,
      IColorValue? ambient,
      IColorValue? specular) {
    var colorOps = equations.ColorOps;

    // Light colors
    var ambientLightColor = colorOps.Multiply(
        equations.CreateOrGetColorInput(
            FixedFunctionSource.LIGHT_AMBIENT_COLOR),
        ambient);
    var diffuseLightColor = equations.GetMergedLightDiffuseColor();
    var specularLightColor
        = colorOps.Multiply(equations.GetMergedLightSpecularColor(), specular);

    var ambientAndDiffuseLightingColor
        = colorOps.Add(ambientLightColor, diffuseLightColor);

    // We double it because all the other kids do. (Other fixed-function games.)
    ambientAndDiffuseLightingColor =
        colorOps.MultiplyWithConstant(ambientAndDiffuseLightingColor, 2);

    var ambientAndDiffuseComponent = colorOps.Multiply(
        ambientAndDiffuseLightingColor,
        diffuse.color);

    // Performs ext lighting pass
    var outColor = colorOps.Add(ambientAndDiffuseComponent, specularLightColor);
    var outAlpha = diffuse.alpha;

    return (outColor, outAlpha);
  }
}