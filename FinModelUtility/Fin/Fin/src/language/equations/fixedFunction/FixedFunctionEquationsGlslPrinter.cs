using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using fin.model;
using fin.shaders.glsl;
using fin.util.asserts;
using fin.util.image;

namespace fin.language.equations.fixedFunction;

public class FixedFunctionEquationsGlslPrinter(IReadOnlyModel model) {
  private readonly IReadOnlyList<IReadOnlyModelAnimation> animations_
      = model.AnimationManager.Animations;

  public string Print(IReadOnlyFixedFunctionMaterial material) {
    var sb = new StringBuilder();
    this.Print(sb, material);
    return sb.ToString();
  }

  public void Print(
      StringBuilder sb,
      IReadOnlyFixedFunctionMaterial material) {
    var equations = material.Equations;
    var registers = material.Registers;
    var textures = material.TextureSources;

    sb.AppendLine("#version 400");
    sb.AppendLine();

    var hasIndividualLights =
        Enumerable
            .Range(0, MaterialConstants.MAX_LIGHTS)
            .Select(
                i => equations.DoOutputsDependOn(
                [
                    FixedFunctionSource.LIGHT_DIFFUSE_COLOR_0 + i,
                    FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_0 + i,
                    FixedFunctionSource.LIGHT_SPECULAR_COLOR_0 + i,
                    FixedFunctionSource.LIGHT_SPECULAR_ALPHA_0 + i
                ]))
            .ToArray();
    var dependsOnAnIndividualLight =
        hasIndividualLights.Any(value => value);
    var dependsOnMergedLights =
        equations.DoOutputsDependOn(
        [
            FixedFunctionSource.LIGHT_DIFFUSE_COLOR_MERGED,
            FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_MERGED,
            FixedFunctionSource.LIGHT_SPECULAR_COLOR_MERGED,
            FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED
        ]);
    var dependsOnLights = dependsOnMergedLights || dependsOnAnIndividualLight;

    var dependsOnAmbientLight = equations.DoOutputsDependOn(
    [
        FixedFunctionSource.LIGHT_AMBIENT_COLOR,
        FixedFunctionSource.LIGHT_AMBIENT_ALPHA
    ]);

    // TODO: Optimize this if we only need ambient
    if (dependsOnLights || dependsOnAmbientLight) {
      sb.AppendLine(GlslUtil.GetLightHeader(dependsOnAmbientLight));
      sb.AppendLine($"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
    }

    var dependsOnIndividualTextures =
        Enumerable
            .Range(0, MaterialConstants.MAX_TEXTURES)
            .Select(
                i => equations
                    .DoOutputsDependOn(
                    [
                        FixedFunctionSource.TEXTURE_COLOR_0 + i,
                        FixedFunctionSource.TEXTURE_ALPHA_0 + i
                    ]))
            .ToArray();
    var dependsOnAnyTextures =
        dependsOnIndividualTextures.Any(value => value);

    sb.AppendTextureStructIfNeeded(
        dependsOnIndividualTextures
            .Select((dependsOnTexture, i) => (i, dependsOnTexture))
            .Where(tuple => tuple.dependsOnTexture)
            .Select(tuple => textures[tuple.i]),
        this.animations_);

    var hadUniform = false;
    for (var t = 0; t < MaterialConstants.MAX_TEXTURES; ++t) {
      if (dependsOnIndividualTextures[t]) {
        hadUniform = true;
        sb.AppendLine(
            $"uniform {GlslUtil.GetTypeOfTexture(textures[t], this.animations_)} texture{t};");
      }
    }

    foreach (var colorRegister in registers.ColorRegisters) {
      if (equations.DoOutputsDependOn(colorRegister)) {
        hadUniform = true;
        sb.AppendLine($"uniform vec3 color_{colorRegister.Name};");
      }
    }

    foreach (var scalarRegister in registers.ScalarRegisters) {
      if (equations.DoOutputsDependOn(scalarRegister)) {
        hadUniform = true;
        sb.AppendLine($"uniform float scalar_{scalarRegister.Name};");
      }
    }

    var hasWrittenLineBetweenUniformsAndIns = false;

    Action AppendLineBetweenUniformsAndIns = () => {
      if (hadUniform && !hasWrittenLineBetweenUniformsAndIns) {
        hasWrittenLineBetweenUniformsAndIns = true;
        sb.AppendLine();
      }
    };

    if (dependsOnAnyTextures &&
        textures.Any(
            texture => texture?.UvType is UvType.SPHERICAL
                                          or UvType.LINEAR)) {
      AppendLineBetweenUniformsAndIns();
      sb.AppendLine("in vec2 normalUv;");
    }

    if (dependsOnLights) {
      AppendLineBetweenUniformsAndIns();
      sb.AppendLine("in vec3 vertexPosition;");
      sb.AppendLine("in vec3 vertexNormal;");
    }

    for (var i = 0; i < MaterialConstants.MAX_COLORS; ++i) {
      if (equations.DoOutputsDependOn([
              FixedFunctionSource.VERTEX_COLOR_0 + i,
              FixedFunctionSource.VERTEX_ALPHA_0 + i
          ])) {
        AppendLineBetweenUniformsAndIns();
        sb.AppendLine($"in vec4 vertexColor{i};");
      }
    }

    for (var i = 0; i < MaterialConstants.MAX_UVS; ++i) {
      if (textures.Any(texture => texture?.UvIndex == i)) {
        AppendLineBetweenUniformsAndIns();
        sb.AppendLine($"in vec2 uv{i};");
      }
    }

    sb.AppendLine();
    sb.AppendLine("out vec4 fragColor;");
    sb.AppendLine();

    if (dependsOnLights) {
      sb.AppendLine(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           """);

      if (dependsOnMergedLights) {
        sb.AppendLine($"""

                       {GlslUtil.GetGetMergedLightColorsFunction()}

                       """);
      }
    }

    sb.AppendLine("void main() {");

    // Calculate lighting
    if (dependsOnLights) {
      sb.AppendLine(
          $"""
             // Have to renormalize because the vertex normals can become distorted when interpolated.
             vec3 fragNormal = normalize(vertexNormal);

           """);
      // TODO: Optimize this if the shader depends on merged lighting as well as individual lights for some reason.
      if (dependsOnAnIndividualLight) {
        sb.AppendLine(
            $$"""
                vec4 individualLightDiffuseColors[{{MaterialConstants.MAX_LIGHTS}}];
                vec4 individualLightSpecularColors[{{MaterialConstants.MAX_LIGHTS}}];
                
                for (int i = 0; i < {{MaterialConstants.MAX_LIGHTS}}; ++i) {
                  vec4 diffuseLightColor = vec4(0);
                  vec4 specularLightColor = vec4(0);
                  
                  getIndividualLightColors(lights[i], vertexPosition, fragNormal, {{GlslConstants.UNIFORM_SHININESS_NAME}}, diffuseLightColor, specularLightColor);
                  
                  individualLightDiffuseColors[i] = diffuseLightColor;
                  individualLightSpecularColors[i] = specularLightColor;
                }
                
              """);
      }

      if (dependsOnMergedLights) {
        sb.AppendLine(
            $"""
               vec4 mergedLightDiffuseColor = vec4(0);
               vec4 mergedLightSpecularColor = vec4(0);
               getMergedLightColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, mergedLightDiffuseColor, mergedLightSpecularColor);
               
             """);
      }
    }

    // TODO: Get tree of all values that this depends on, in case there needs to be other variables defined before.
    var outputColor =
        equations.ColorOutputs[FixedFunctionSource.OUTPUT_COLOR];

    sb.Append("  vec3 colorComponent = ");
    this.PrintColorValue_(sb, outputColor.ColorValue, textures);
    sb.AppendLine(";");
    sb.AppendLine();

    var outputAlpha =
        equations.ScalarOutputs[FixedFunctionSource.OUTPUT_ALPHA];

    sb.Append("  float alphaComponent = ");
    this.PrintScalarValue_(sb, outputAlpha.ScalarValue, textures);
    sb.AppendLine(";");
    sb.AppendLine();

    if (material.TransparencyType == TransparencyType.TRANSPARENT) {
      sb.AppendLine("  fragColor = vec4(colorComponent, alphaComponent);");
    } else {
      sb.AppendLine("  fragColor = vec4(colorComponent, 1);");
    }

    var alphaOpValue =
        this.DetermineAlphaOpValue_(
            material.AlphaOp,
            this.DetermineAlphaCompareType_(
                material.AlphaCompareType0,
                material.AlphaReference0),
            this.DetermineAlphaCompareType_(
                material.AlphaCompareType1,
                material.AlphaReference1));

    if (alphaOpValue != AlphaOpValue.ALWAYS_TRUE) {
      sb.AppendLine();

      var alphaCompareText0 =
          this.GetAlphaCompareText_(material.AlphaCompareType0,
                                    "alphaComponent",
                                    material.AlphaReference0);
      var alphaCompareText1 =
          this.GetAlphaCompareText_(material.AlphaCompareType1,
                                    "alphaComponent",
                                    material.AlphaReference1);

      switch (alphaOpValue) {
        case AlphaOpValue.ONLY_0_REQUIRED: {
          sb.AppendLine($@"  if (!({alphaCompareText0})) {{
    discard;
  }}");
          break;
        }
        case AlphaOpValue.ONLY_1_REQUIRED: {
          sb.AppendLine($@"  if (!({alphaCompareText1})) {{
    discard;
  }}");
          break;
        }
        case AlphaOpValue.BOTH_REQUIRED: {
          switch (material.AlphaOp) {
            case AlphaOp.And: {
              sb.Append(
                  $"  if (!({alphaCompareText0} && {alphaCompareText1})");
              break;
            }
            case AlphaOp.Or: {
              sb.Append(
                  $"  if (!({alphaCompareText0} || {alphaCompareText1})");
              break;
            }
            case AlphaOp.XOR: {
              sb.AppendLine($"  bool a = {alphaCompareText0};");
              sb.AppendLine($"  bool b = {alphaCompareText1};");
              sb.Append(
                  $"  if (!(any(bvec2(all(bvec2(!a, b)), all(bvec2(a, !b)))))");
              break;
            }
            case AlphaOp.XNOR: {
              sb.AppendLine($"  bool a = {alphaCompareText0};");
              sb.AppendLine($"  bool b = {alphaCompareText1};");
              sb.Append(
                  "  if (!(any(bvec2(all(bvec2(!a, !b)), all(bvec2(a, b)))))");
              break;
            }
            default: throw new ArgumentOutOfRangeException();
          }

          sb.AppendLine(@") {
    discard;
  }");
          break;
        }
        case AlphaOpValue.ALWAYS_FALSE: {
          sb.AppendLine("  discard;");
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }

    sb.AppendLine("}");
  }

  private string GetAlphaCompareText_(
      AlphaCompareType alphaCompareType,
      string alphaAccessorText,
      float reference)
    => alphaCompareType switch {
        AlphaCompareType.Never   => "false",
        AlphaCompareType.Less    => $"{alphaAccessorText} < {reference}",
        AlphaCompareType.Equal   => $"{alphaAccessorText} == {reference}",
        AlphaCompareType.LEqual  => $"{alphaAccessorText} <= {reference}",
        AlphaCompareType.Greater => $"{alphaAccessorText} > {reference}",
        AlphaCompareType.NEqual  => $"{alphaAccessorText} != {reference}",
        AlphaCompareType.GEqual  => $"{alphaAccessorText} >= {reference}",
        AlphaCompareType.Always  => "true",
        _ => throw new ArgumentOutOfRangeException(
            nameof(alphaCompareType),
            alphaCompareType,
            null)
    };

  private enum AlphaOpValue {
    ONLY_0_REQUIRED,
    ONLY_1_REQUIRED,
    BOTH_REQUIRED,
    ALWAYS_TRUE,
    ALWAYS_FALSE,
  }

  private AlphaOpValue DetermineAlphaOpValue_(
      AlphaOp alphaOp,
      AlphaCompareValue compareValue0,
      AlphaCompareValue compareValue1) {
    var is0False = compareValue0 == AlphaCompareValue.ALWAYS_FALSE;
    var is0True = compareValue0 == AlphaCompareValue.ALWAYS_TRUE;
    var is1False = compareValue1 == AlphaCompareValue.ALWAYS_FALSE;
    var is1True = compareValue1 == AlphaCompareValue.ALWAYS_TRUE;

    if (alphaOp == AlphaOp.And) {
      if (is0False || is1False) {
        return AlphaOpValue.ALWAYS_FALSE;
      }

      if (is0True && is1True) {
        return AlphaOpValue.ALWAYS_TRUE;
      }

      if (is0True) {
        return AlphaOpValue.ONLY_1_REQUIRED;
      }

      if (is1True) {
        return AlphaOpValue.ONLY_0_REQUIRED;
      }

      return AlphaOpValue.BOTH_REQUIRED;
    }

    if (alphaOp == AlphaOp.Or) {
      if (is0True || is1True) {
        return AlphaOpValue.ALWAYS_TRUE;
      }

      if (is0False && is1False) {
        return AlphaOpValue.ALWAYS_FALSE;
      }

      if (is0False) {
        return AlphaOpValue.ONLY_1_REQUIRED;
      }

      if (is1False) {
        return AlphaOpValue.ONLY_0_REQUIRED;
      }

      return AlphaOpValue.BOTH_REQUIRED;
    }

    return AlphaOpValue.BOTH_REQUIRED;
  }

  private enum AlphaCompareValue {
    INDETERMINATE,
    ALWAYS_TRUE,
    ALWAYS_FALSE,
  }

  private AlphaCompareValue DetermineAlphaCompareType_(
      AlphaCompareType compareType,
      float reference) {
    var isReference0 = Math.Abs(reference - 0) < .001;
    var isReference1 = Math.Abs(reference - 1) < .001;

    if (compareType == AlphaCompareType.Always ||
        (compareType == AlphaCompareType.GEqual && isReference0) ||
        (compareType == AlphaCompareType.LEqual && isReference1)) {
      return AlphaCompareValue.ALWAYS_TRUE;
    }

    if (compareType == AlphaCompareType.Never ||
        (compareType == AlphaCompareType.Greater && isReference1) ||
        (compareType == AlphaCompareType.Less && isReference0)) {
      return AlphaCompareValue.ALWAYS_FALSE;
    }

    return AlphaCompareValue.INDETERMINATE;
  }

  private void PrintScalarValue_(
      StringBuilder sb,
      IScalarValue value,
      IReadOnlyList<IReadOnlyTexture> textures,
      bool wrapExpressions = false) {
    if (value is IScalarExpression expression) {
      if (wrapExpressions) {
        sb.Append("(");
      }

      this.PrintScalarExpression_(sb, expression, textures);
      if (wrapExpressions) {
        sb.Append(")");
      }
    } else if (value is IScalarTerm term) {
      this.PrintScalarTerm_(sb, term, textures);
    } else if (value is IScalarFactor factor) {
      this.PrintScalarFactor_(sb, factor, textures);
    } else {
      Asserts.Fail("Unsupported value type!");
    }
  }

  private void PrintScalarExpression_(
      StringBuilder sb,
      IScalarExpression expression,
      IReadOnlyList<IReadOnlyTexture> textures) {
    var terms = expression.Terms;

    for (var i = 0; i < terms.Count; ++i) {
      var term = terms[i];

      if (i > 0) {
        sb.Append(" + ");
      }

      this.PrintScalarValue_(sb, term, textures);
    }
  }

  private void PrintScalarTerm_(
      StringBuilder sb,
      IScalarTerm scalarTerm,
      IReadOnlyList<IReadOnlyTexture> textures) {
    var numerators = scalarTerm.NumeratorFactors;
    var denominators = scalarTerm.DenominatorFactors;

    if (numerators.Count > 0) {
      for (var i = 0; i < numerators.Count; ++i) {
        var numerator = numerators[i];

        if (i > 0) {
          sb.Append("*");
        }

        this.PrintScalarValue_(sb, numerator, textures, true);
      }
    } else {
      sb.Append(1);
    }

    if (denominators != null) {
      for (var i = 0; i < denominators.Count; ++i) {
        var denominator = denominators[i];

        sb.Append("/");

        this.PrintScalarValue_(sb, denominator, textures, true);
      }
    }
  }

  private void PrintScalarFactor_(
      StringBuilder sb,
      IScalarFactor factor,
      IReadOnlyList<IReadOnlyTexture> textures) {
    if (factor is IScalarIdentifiedValue<FixedFunctionSource>
        identifiedValue) {
      this.PrintScalarIdentifiedValue_(sb, identifiedValue, textures);
    } else if (factor is IScalarNamedValue namedValue) {
      this.PrintScalarNamedValue_(sb, namedValue);
    } else if (factor is IScalarConstant constant) {
      this.PrintScalarConstant_(sb, constant);
    } else if
        (factor is IColorNamedValueSwizzle<FixedFunctionSource>
         namedSwizzle) {
      this.PrintColorNamedValueSwizzle_(sb, namedSwizzle, textures);
    } else if (factor is IColorValueSwizzle swizzle) {
      this.PrintColorValueSwizzle_(sb, swizzle, textures);
    } else {
      Asserts.Fail("Unsupported factor type!");
    }
  }

  private void PrintScalarNamedValue_(
      StringBuilder sb,
      IScalarNamedValue namedValue)
    => sb.Append($"scalar_{namedValue.Name}");

  private void PrintScalarIdentifiedValue_(
      StringBuilder sb,
      IScalarIdentifiedValue<FixedFunctionSource> identifiedValue,
      IReadOnlyList<IReadOnlyTexture> textures)
    => sb.Append(this.GetScalarIdentifiedValue_(identifiedValue, textures));

  private string GetScalarIdentifiedValue_(
      IScalarIdentifiedValue<FixedFunctionSource> identifiedValue,
      IReadOnlyList<IReadOnlyTexture> textures) {
    var id = identifiedValue.Identifier;
    var isTextureAlpha = id is >= FixedFunctionSource.TEXTURE_ALPHA_0
                               and <= FixedFunctionSource.TEXTURE_ALPHA_7;

    if (isTextureAlpha) {
      var textureIndex =
          (int) id - (int) FixedFunctionSource.TEXTURE_ALPHA_0;

      var textureText = this.GetTextureValue_(textureIndex, textures);
      var textureValueText = $"{textureText}.a";

      return textureValueText;
    }

    if (IsInRange_(id,
                   FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_0,
                   FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_7,
                   out var globalDiffuseAlphaIndex)) {
      return $"individualLightDiffuseColors[{globalDiffuseAlphaIndex}].a";
    }

    if (IsInRange_(id,
                   FixedFunctionSource.LIGHT_SPECULAR_ALPHA_0,
                   FixedFunctionSource.LIGHT_SPECULAR_ALPHA_7,
                   out var globalSpecularAlphaIndex)) {
      return $"individualLightSpecularColors[{globalSpecularAlphaIndex}].a";
    }

    return identifiedValue.Identifier switch {
        FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_MERGED =>
            "mergedLightDiffuseColor.a",
        FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED =>
            "mergedLightSpecularColor.a",

        FixedFunctionSource.LIGHT_AMBIENT_ALPHA => "ambientLightColor.a",

        FixedFunctionSource.VERTEX_ALPHA_0 => "vertexColor0.a",
        FixedFunctionSource.VERTEX_ALPHA_1 => "vertexColor1.a",

        FixedFunctionSource.UNDEFINED => "1",
        _                             => throw new ArgumentOutOfRangeException()
    };
  }

  private void PrintScalarConstant_(
      StringBuilder sb,
      IScalarConstant constant)
    => sb.Append(constant.Value);

  private enum WrapType {
    NEVER,
    EXPRESSIONS,
    ALWAYS
  }

  private void PrintColorValue_(
      StringBuilder sb,
      IColorValue value,
      IReadOnlyList<IReadOnlyTexture> textures,
      WrapType wrapType = WrapType.NEVER) {
    var clamp = value.Clamp;

    if (clamp) {
      sb.Append("clamp(");
    }

    if (value is IColorExpression expression) {
      var wrapExpressions =
          wrapType is WrapType.EXPRESSIONS or WrapType.ALWAYS;
      if (wrapExpressions) {
        sb.Append("(");
      }

      this.PrintColorExpression_(sb, expression, textures);
      if (wrapExpressions) {
        sb.Append(")");
      }
    } else if (value is IColorTerm term) {
      var wrapTerms = wrapType == WrapType.ALWAYS;
      if (wrapTerms) {
        sb.Append("(");
      }

      this.PrintColorTerm_(sb, term, textures);
      if (wrapTerms) {
        sb.Append(")");
      }
    } else if (value is IColorFactor factor) {
      var wrapFactors = wrapType == WrapType.ALWAYS;
      if (wrapFactors) {
        sb.Append("(");
      }

      this.PrintColorFactor_(sb, factor, textures);
      if (wrapFactors) {
        sb.Append(")");
      }
    } else if (value is IColorValueTernaryOperator ternaryOperator) {
      this.PrintColorTernaryOperator_(sb, ternaryOperator, textures);
    } else {
      Asserts.Fail("Unsupported value type!");
    }

    if (clamp) {
      sb.Append(", 0, 1)");
    }
  }

  private void PrintColorExpression_(
      StringBuilder sb,
      IColorExpression expression,
      IReadOnlyList<IReadOnlyTexture> textures) {
    var terms = expression.Terms;

    for (var i = 0; i < terms.Count; ++i) {
      var term = terms[i];

      if (i > 0) {
        sb.Append(" + ");
      }

      this.PrintColorValue_(sb, term, textures);
    }
  }

  private void PrintColorTerm_(
      StringBuilder sb,
      IColorTerm scalarTerm,
      IReadOnlyList<IReadOnlyTexture> textures) {
    var numerators = scalarTerm.NumeratorFactors;
    var denominators = scalarTerm.DenominatorFactors;

    if (numerators.Count > 0) {
      for (var i = 0; i < numerators.Count; ++i) {
        var numerator = numerators[i];

        if (i > 0) {
          sb.Append("*");
        }

        this.PrintColorValue_(sb, numerator, textures, WrapType.EXPRESSIONS);
      }
    } else {
      sb.Append(1);
    }

    if (denominators != null) {
      for (var i = 0; i < denominators.Count; ++i) {
        var denominator = denominators[i];

        sb.Append("/");

        this.PrintColorValue_(sb,
                              denominator,
                              textures,
                              WrapType.EXPRESSIONS);
      }
    }
  }

  private void PrintColorFactor_(
      StringBuilder sb,
      IColorFactor factor,
      IReadOnlyList<IReadOnlyTexture> textures) {
    if (factor is IColorIdentifiedValue<FixedFunctionSource>
        identifiedValue) {
      this.PrintColorIdentifiedValue_(sb, identifiedValue, textures);
    } else if (factor is IColorNamedValue namedValue) {
      this.PrintColorNamedValue_(sb, namedValue);
    } else {
      var useIntensity = factor.Intensity != null;

      if (!useIntensity) {
        var r = factor.R;
        var g = factor.G;
        var b = factor.B;

        sb.Append("vec3(");
        this.PrintScalarValue_(sb, r, textures);
        sb.Append(",");
        this.PrintScalarValue_(sb, g, textures);
        sb.Append(",");
        this.PrintScalarValue_(sb, b, textures);
        sb.Append(")");
      } else {
        sb.Append("vec3(");
        this.PrintScalarValue_(sb, factor.Intensity!, textures);
        sb.Append(")");
      }
    }
  }

  private void PrintColorIdentifiedValue_(
      StringBuilder sb,
      IColorIdentifiedValue<FixedFunctionSource> identifiedValue,
      IReadOnlyList<IReadOnlyTexture> textures)
    => sb.Append(this.GetColorNamedValue_(identifiedValue, textures));

  private void PrintColorNamedValue_(
      StringBuilder sb,
      IColorNamedValue namedValue)
    => sb.Append($"color_{namedValue.Name}");

  private string GetColorNamedValue_(
      IColorIdentifiedValue<FixedFunctionSource> identifiedValue,
      IReadOnlyList<IReadOnlyTexture> textures) {
    var id = identifiedValue.Identifier;
    var isTextureColor = id is >= FixedFunctionSource.TEXTURE_COLOR_0
                               and <= FixedFunctionSource.TEXTURE_COLOR_7;
    var isTextureAlpha = id is >= FixedFunctionSource.TEXTURE_ALPHA_0
                               and <= FixedFunctionSource.TEXTURE_ALPHA_7;

    if (isTextureColor || isTextureAlpha) {
      var textureIndex =
          isTextureColor
              ? (int) id - (int) FixedFunctionSource.TEXTURE_COLOR_0
              : (int) id - (int) FixedFunctionSource.TEXTURE_ALPHA_0;

      var textureText = this.GetTextureValue_(textureIndex, textures);
      var textureValueText = isTextureColor
          ? $"{textureText}.rgb"
          : $"vec3({textureText}.a)";

      return textureValueText;
    }

    if (IsInRange_(id,
                   FixedFunctionSource.LIGHT_DIFFUSE_COLOR_0,
                   FixedFunctionSource.LIGHT_DIFFUSE_COLOR_7,
                   out var globalDiffuseColorIndex)) {
      return $"individualLightDiffuseColors[{globalDiffuseColorIndex}].rgb";
    }

    if (IsInRange_(id,
                   FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_0,
                   FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_7,
                   out var globalDiffuseAlphaIndex)) {
      return $"individualLightDiffuseColors[{globalDiffuseAlphaIndex}].aaa";
    }

    if (IsInRange_(id,
                   FixedFunctionSource.LIGHT_SPECULAR_COLOR_0,
                   FixedFunctionSource.LIGHT_SPECULAR_COLOR_7,
                   out var globalSpecularColorIndex)) {
      return $"individualLightSpecularColors[{globalSpecularColorIndex}].rgb";
    }

    if (IsInRange_(id,
                   FixedFunctionSource.LIGHT_SPECULAR_ALPHA_0,
                   FixedFunctionSource.LIGHT_SPECULAR_ALPHA_7,
                   out var globalSpecularAlphaIndex)) {
      return $"individualLightSpecularColors[{globalSpecularAlphaIndex}].aaa";
    }

    return identifiedValue.Identifier switch {
        FixedFunctionSource.LIGHT_DIFFUSE_COLOR_MERGED =>
            "mergedLightDiffuseColor.rgb",
        FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_MERGED =>
            "mergedLightDiffuseColor.aaa",
        FixedFunctionSource.LIGHT_SPECULAR_COLOR_MERGED =>
            "mergedLightSpecularColor.rgb",
        FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED =>
            "mergedLightSpecularColor.aaa",

        FixedFunctionSource.LIGHT_AMBIENT_COLOR => "ambientLightColor.rgb",
        FixedFunctionSource.LIGHT_AMBIENT_ALPHA => "ambientLightColor.aaa",

        FixedFunctionSource.VERTEX_COLOR_0 => "vertexColor0.rgb",
        FixedFunctionSource.VERTEX_COLOR_1 => "vertexColor1.rgb",

        FixedFunctionSource.VERTEX_ALPHA_0 => "vertexColor0.aaa",
        FixedFunctionSource.VERTEX_ALPHA_1 => "vertexColor1.aaa",

        FixedFunctionSource.UNDEFINED => "vec3(1)",
        _                             => throw new ArgumentOutOfRangeException()
    };
  }

  private bool IsInRange_(FixedFunctionSource value,
                          FixedFunctionSource min,
                          FixedFunctionSource max,
                          out int relative) {
    relative = value - min;
    return value >= min && value <= max;
  }

  private string GetTextureValue_(int textureIndex,
                                  IReadOnlyList<IReadOnlyTexture> textures) {
    var texture = textures[textureIndex];
    var textureName = $"texture{textureIndex}";
    return texture.UvType switch {
        UvType.STANDARD
            => GlslUtil.ReadColorFromTexture(textureName,
                                             $"uv{texture.UvIndex}",
                                             texture,
                                             this.animations_),
        UvType.SPHERICAL
            => GlslUtil.ReadColorFromTexture(
                textureName,
                "normalUv",
                uv => $"asin({uv}) / 3.14159 + 0.5",
                texture,
                this.animations_),
        UvType.LINEAR
            => GlslUtil.ReadColorFromTexture(
                textureName,
                "normalUv",
                uv => $"acos({uv}) / 3.14159",
                texture,
                this.animations_),
    };
  }

  private void PrintColorTernaryOperator_(
      StringBuilder sb,
      IColorValueTernaryOperator ternaryOperator,
      IReadOnlyList<IReadOnlyTexture> textures) {
    sb.Append('(');
    switch (ternaryOperator.ComparisonType) {
      case BoolComparisonType.EQUAL_TO: {
        sb.Append("abs(");
        this.PrintScalarValue_(sb, ternaryOperator.Lhs, textures);
        sb.Append(" - ");
        this.PrintScalarValue_(sb, ternaryOperator.Rhs, textures);
        sb.Append(")");
        sb.Append(" < ");
        sb.Append("(1.0 / 255)");
        break;
      }
      case BoolComparisonType.GREATER_THAN: {
        this.PrintScalarValue_(sb, ternaryOperator.Lhs, textures);
        sb.Append(" > ");
        this.PrintScalarValue_(sb, ternaryOperator.Rhs, textures);
        break;
      }
      default:
        throw new ArgumentOutOfRangeException(
            nameof(ternaryOperator.ComparisonType));
    }

    sb.Append(" ? ");
    this.PrintColorValue_(sb, ternaryOperator.TrueValue, textures);
    sb.Append(" : ");
    this.PrintColorValue_(sb, ternaryOperator.FalseValue, textures);
    sb.Append(')');
  }

  private void PrintColorNamedValueSwizzle_(
      StringBuilder sb,
      IColorNamedValueSwizzle<FixedFunctionSource> swizzle,
      IReadOnlyList<IReadOnlyTexture> textures) {
    this.PrintColorIdentifiedValue_(sb, swizzle.Source, textures);
    sb.Append(".");
    sb.Append(swizzle.SwizzleType switch {
        ColorSwizzle.R => 'r',
        ColorSwizzle.G => 'g',
        ColorSwizzle.B => 'b',
    });
  }

  private void PrintColorValueSwizzle_(
      StringBuilder sb,
      IColorValueSwizzle swizzle,
      IReadOnlyList<IReadOnlyTexture> textures) {
    this.PrintColorValue_(sb, swizzle.Source, textures, WrapType.ALWAYS);
    sb.Append(".");
    sb.Append(swizzle.SwizzleType);
  }
}