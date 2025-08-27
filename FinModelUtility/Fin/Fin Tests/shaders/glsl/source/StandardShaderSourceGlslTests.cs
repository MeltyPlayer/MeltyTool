using System;
using System.Drawing;

using fin.image;
using fin.model;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl.source;

public class StandardShaderSourceGlslTests {
  [Test]
  public void TestWithoutNothing()
    => AssertGlsl_(
        default,
        (m, t) => { },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          out vec4 fragColor;

          void main() {
            fragColor = vec4(1);
          
            if (fragColor.a < .01) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithoutNormalsNoTextures()
    => AssertGlsl_(
        new MockMaterialOptions {
            Masked = true,
            WithNormals = false,
        },
        (m, t) => { },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          out vec4 fragColor;

          void main() {
            fragColor = vec4(1);
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithVertexColor()
    => AssertGlsl_(
        new MockMaterialOptions { WithColors = true, },
        (m, t) => { },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          out vec4 fragColor;

          in vec4 vertexColor0;

          void main() {
            fragColor = vertexColor0;
          
            if (fragColor.a < .01) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithoutNormalsDiffuseOnly()
    => AssertGlsl_(
        new MockMaterialOptions {
            Masked = true,
            WithNormals = false,
            WithUvs = true,
        },
        (m, t) => m.DiffuseTexture = t,
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          uniform sampler2D diffuseTexture;

          out vec4 fragColor;

          in vec2 uv0;

          void main() {
            fragColor = texture(diffuseTexture, uv0);
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithoutNormalsEmissiveOnly()
    => AssertGlsl_(
        new MockMaterialOptions {
            Masked = true,
            WithNormals = false,
            WithUvs = true,
        },
        (m, t) => m.EmissiveTexture = t,
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          uniform sampler2D emissiveTexture;

          out vec4 fragColor;

          in vec2 uv0;

          void main() {
            fragColor = vec4(1);
          
            vec4 emissiveColor = texture(emissiveTexture, uv0);
            fragColor.rgb += emissiveColor.rgb;
            fragColor.rgb = min(fragColor.rgb, 1.0);
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithNormalsNoTextures()
    => AssertGlsl_(
        new MockMaterialOptions {
            Masked = true,
            WithNormals = true,
        },
        (m, t) => { },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          {{GlslUtil.GetLightHeader(true)}}

          uniform float shininess;

          out vec4 fragColor;

          in vec3 vertexPosition;
          in vec3 vertexNormal;

          {{GlslUtil.GetGetIndividualLightColorsFunction()}}

          {{GlslUtil.GetGetMergedLightColorsFunction()}}

          {{GlslUtil.GetApplyMergedLightColorsFunction(false)}}

          void main() {
            fragColor = vec4(1);
          
            // Have to renormalize because the vertex normals can become distorted when interpolated.
            vec3 fragNormal = normalize(vertexNormal);
            fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, shininess, fragColor, vec4(1)).rgb, useLighting);
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithNormalsDiffuseOnly()
    => AssertGlsl_(
        new MockMaterialOptions {
            Masked = true,
            WithNormals = true,
            WithUvs = true,
        },
        (m, t) => m.DiffuseTexture = t,
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          {{GlslUtil.LIGHT_HEADER}}

          uniform sampler2D diffuseTexture;
          uniform float shininess;

          out vec4 fragColor;

          in vec3 vertexPosition;
          in vec3 vertexNormal;
          in vec2 uv0;

          {{GlslUtil.GetGetIndividualLightColorsFunction()}}

          {{GlslUtil.GetGetMergedLightColorsFunction()}}

          {{GlslUtil.GetApplyMergedLightColorsFunction(false)}}

          void main() {
            fragColor = texture(diffuseTexture, uv0);
          
            // Have to renormalize because the vertex normals can become distorted when interpolated.
            vec3 fragNormal = normalize(vertexNormal);
            fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, shininess, fragColor, vec4(1)).rgb, useLighting);
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithNormalsEmissiveOnly()
    => AssertGlsl_(
        new MockMaterialOptions {
            Masked = true,
            WithNormals = true,
            WithUvs = true,
        },
        (m, t) => m.EmissiveTexture = t,
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          {{GlslUtil.LIGHT_HEADER}}

          uniform sampler2D emissiveTexture;
          uniform float shininess;

          out vec4 fragColor;

          in vec3 vertexPosition;
          in vec3 vertexNormal;
          in vec2 uv0;

          {{GlslUtil.GetGetIndividualLightColorsFunction()}}

          {{GlslUtil.GetGetMergedLightColorsFunction()}}

          {{GlslUtil.GetApplyMergedLightColorsFunction(false)}}

          void main() {
            fragColor = vec4(1);
          
            // Have to renormalize because the vertex normals can become distorted when interpolated.
            vec3 fragNormal = normalize(vertexNormal);
            fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, shininess, fragColor, vec4(1)).rgb, useLighting);
          
            vec4 emissiveColor = texture(emissiveTexture, uv0);
            fragColor.rgb += emissiveColor.rgb;
            fragColor.rgb = min(fragColor.rgb, 1.0);
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  private static void AssertGlsl_(
      MockMaterialOptions options,
      Action<IStandardMaterial, IReadOnlyTexture> createMaterial,
      string expectedSource)
    => Assert.AreEqual(
        expectedSource,
        MockMaterial.BuildAndGetSource(
                        options,
                        mm => {
                          var texture = mm.CreateTexture(
                              FinImage.Create1x1FromColor(Color.White));
                          var material = mm.AddStandardMaterial();
                          createMaterial(material, texture);
                          return material;
                        })
                    .FragmentShaderSource);
}