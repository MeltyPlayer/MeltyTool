using System;
using System.Drawing;

using fin.image;
using fin.language.equations.fixedFunction;
using fin.language.equations.fixedFunction.impl;
using fin.model;
using fin.model.impl;
using fin.model.util;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl;

public class FixedFunctionShaderSourceGlslTests {
  [Test]
  public void TestWithNothing()
    => AssertGlsl_(
        false,
        false,
        (m, t) => CreateFixedFunctionMaterial_(m, t, false, false, false),
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          out vec4 fragColor;

          void main() {
            vec3 colorComponent = vec3(1.0);
          
            float alphaComponent = 1.0;
          
            fragColor = vec4(colorComponent, alphaComponent);
          }
          """);

  [Test]
  public void TestWithVertexColorOnly()
    => AssertGlsl_(
        false,
        false,
        (m, t) => CreateFixedFunctionMaterial_(m, t, false, false, true),
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          in vec4 vertexColor0;

          out vec4 fragColor;

          void main() {
            vec3 colorComponent = vertexColor0.rgb;
          
            float alphaComponent = vertexColor0.a;
          
            fragColor = vec4(colorComponent, alphaComponent);
          }
          """);

  [Test]
  public void TestWithTextureOnly()
    => AssertGlsl_(
        false,
        true,
        (m, t) => CreateFixedFunctionMaterial_(m, t, false, true, false),
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          uniform sampler2D texture0;

          in vec2 uv0;

          out vec4 fragColor;

          void main() {
            vec3 colorComponent = texture(texture0, uv0).rgb;
          
            float alphaComponent = texture(texture0, uv0).a;
          
            fragColor = vec4(colorComponent, alphaComponent);
          
            if (!(alphaComponent > 0.95)) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithLightingAndTextureAndVertexColor()
    => AssertGlsl_(
        true,
        true,
        (m, t) => CreateFixedFunctionMaterial_(m, t, true, true, true),
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          {{GlslUtil.GetLightHeader(true)}}
          uniform float shininess;
          uniform sampler2D texture0;

          in vec3 vertexPosition;
          in vec3 vertexNormal;
          in vec4 vertexColor0;
          in vec2 uv0;

          out vec4 fragColor;

          {{GlslUtil.GetGetIndividualLightColorsFunction()}}

          {{GlslUtil.GetGetMergedLightColorsFunction()}}

          void main() {
            // Have to renormalize because the vertex normals can become distorted when interpolated.
            vec3 fragNormal = normalize(vertexNormal);
          
            vec4 mergedLightDiffuseColor = vec4(0);
            vec4 mergedLightSpecularColor = vec4(0);
            getMergedLightColors(vertexPosition, fragNormal, shininess, mergedLightDiffuseColor, mergedLightSpecularColor);
          
            vec3 colorComponent = vertexColor0.rgb*texture(texture0, uv0).rgb*mergedLightDiffuseColor.rgb;
          
            float alphaComponent = vertexColor0.a*texture(texture0, uv0).a;
          
            fragColor = vec4(colorComponent, alphaComponent);
          
            if (!(alphaComponent > 0.95)) {
              discard;
            }
          }
          """);

  private static void AssertGlsl_(
      bool withNormals,
      bool masked,
      Action<IFixedFunctionMaterial, IReadOnlyTexture> createMaterial,
      string expectedSource) {
    var model = ModelImpl.CreateForViewer();

    var materialManager = model.MaterialManager;
    var material = materialManager.AddFixedFunctionMaterial();
    var texture = materialManager.CreateTexture(
        FinImage.Create1x1FromColor(Color.White));
    createMaterial(material, texture);

    if (withNormals) {
      var skin = model.Skin;

      var v = skin.AddVertex(0, 0, 0);
      v.SetLocalNormal(0, 0, 1);

      skin.AddMesh().AddPoints(v).SetMaterial(material);
    }

    if (masked) {
      material.SetAlphaCompare(AlphaOp.Or,
                               AlphaCompareType.Greater,
                               .95f,
                               AlphaCompareType.Never,
                               0);
    }

    var actualSource = new FixedFunctionShaderSourceGlsl(
        model,
        material,
        true,
        ShaderRequirements.FromModelAndMaterial(
            model,
            material)).FragmentShaderSource;

    Assert.AreEqual(expectedSource, actualSource);
  }

  private static void CreateFixedFunctionMaterial_(
      IFixedFunctionMaterial material,
      IReadOnlyTexture texture,
      bool withLighting,
      bool withTexture,
      bool withVertexColor) {
    material.SetTextureSource(0, texture);

    var equations = material.Equations;

    var colorOps = new ColorFixedFunctionOps(equations);
    var scalarOps = new ScalarFixedFunctionOps(equations);

    IColorValue? outputColor = colorOps.One;
    IScalarValue? outputAlpha = scalarOps.One;

    if (withVertexColor) {
      outputColor = colorOps.Multiply(
          outputColor,
          equations.CreateOrGetColorInput(FixedFunctionSource.VERTEX_COLOR_0));
      outputAlpha = scalarOps.Multiply(
          outputAlpha,
          equations.CreateOrGetScalarInput(FixedFunctionSource.VERTEX_ALPHA_0));
    }

    if (withTexture) {
      outputColor = colorOps.Multiply(
          outputColor,
          equations.CreateOrGetColorInput(FixedFunctionSource.TEXTURE_COLOR_0));
      outputAlpha = scalarOps.Multiply(
          outputAlpha,
          equations.CreateOrGetScalarInput(
              FixedFunctionSource.TEXTURE_ALPHA_0));
    }

    if (withLighting) {
      outputColor = colorOps.Multiply(outputColor,
                                      equations.GetMergedLightDiffuseColor());
    }

    equations.CreateColorOutput(FixedFunctionSource.OUTPUT_COLOR,
                                outputColor ?? colorOps.Zero);
    equations.CreateScalarOutput(FixedFunctionSource.OUTPUT_ALPHA,
                                 outputAlpha ?? scalarOps.Zero);
  }
}