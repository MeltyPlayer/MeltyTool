using fin.model;

namespace fin.language.equations.fixedFunction;

public static partial class FixedFunctionEquationsExtensions {
  public static (IColorValue?, IScalarValue?) GenerateDiffuse(
      this IFixedFunctionMaterial material,
      (IColorValue? color, IScalarValue? alpha) diffuse,
      IReadOnlyTexture? finTexture,
      (bool color, bool alpha) hasVertexColorAlpha0) {
    var equations = material.Equations;
    var colorOps = equations.ColorOps;
    var scalarOps = equations.ScalarOps;

    IColorValue? diffuseSurfaceColor = diffuse.color;
    IScalarValue? diffuseSurfaceAlpha = diffuse.alpha;

    if (finTexture != null) {
      var (textureColor, textureAlpha)
          = material.AddTextureSourceColorAlpha(finTexture);

      diffuseSurfaceColor
          = colorOps.Multiply(diffuseSurfaceColor, textureColor);
      diffuseSurfaceAlpha
          = scalarOps.Multiply(diffuseSurfaceAlpha, textureAlpha);
    }

    if (hasVertexColorAlpha0.color) {
      var vertexColor = equations.CreateOrGetColorInput(
          FixedFunctionSource.VERTEX_COLOR_0);
      diffuseSurfaceColor
          = colorOps.Multiply(diffuseSurfaceColor, vertexColor);
    }

    if (hasVertexColorAlpha0.alpha) {
      var vertexAlpha = equations.CreateOrGetScalarInput(
          FixedFunctionSource.VERTEX_ALPHA_0);
      diffuseSurfaceAlpha
          = scalarOps.Multiply(diffuseSurfaceAlpha, vertexAlpha);
    }

    return (diffuseSurfaceColor, diffuseSurfaceAlpha);
  }

  public static (IColorValue?, IScalarValue?) GenerateLighting(
      this IFixedFunctionEquations<FixedFunctionSource> equations,
      (IColorValue? color, IScalarValue? alpha) diffuse,
      IColorValue? ambient)
    => GenerateLighting(equations,
                        diffuse,
                        ambient,
                        equations.ColorOps.Zero,
                        equations.ColorOps.Zero);

  public static (IColorValue?, IScalarValue?) GenerateLighting(
      this IFixedFunctionEquations<FixedFunctionSource> equations,
      (IColorValue? color, IScalarValue? alpha) diffuse,
      IColorValue? ambient,
      IColorValue? specular,
      IColorValue? emission) {
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
    var outColor = colorOps.Add(
        colorOps.Add(ambientAndDiffuseComponent, specularLightColor),
        emission);
    var outAlpha = diffuse.alpha;

    return (outColor, outAlpha);
  }
}