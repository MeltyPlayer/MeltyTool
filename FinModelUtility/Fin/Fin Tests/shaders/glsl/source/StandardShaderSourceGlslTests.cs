using System;
using System.Drawing;

using fin.image;
using fin.image.util;
using fin.model;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl.source;

public sealed class StandardShaderSourceGlslTests {
  [Test]
  public void TestWithoutNothing()
    => AssertGlsl_(
        default,
        (m, t) => { },
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

          out vec4 fragColor;

          void main() {
            fragColor = vec4(1);
          }
          """);

  [Test]
  public void TestWithoutNormalsNoTextures()
    => AssertGlsl_(
        new MockMaterialOptions {
            WithNormals = false,
        },
        (m, t) => { },
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

          out vec4 fragColor;

          void main() {
            fragColor = vec4(1);
          }
          """);

  [Test]
  public void TestWithVertexColor()
    => AssertGlsl_(
        new MockMaterialOptions { WithColors = true, },
        (m, t) => { },
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

          out vec4 fragColor;

          in vec4 vertexColor0;

          void main() {
            fragColor = vertexColor0;
          }
          """);

  [Test]
  public void TestWithoutNormalsDiffuseOnly()
    => AssertGlsl_(
        new MockMaterialOptions {
            TransparencyType = TransparencyType.MASK,
            WithNormals = false,
            WithUvs = true,
        },
        (m, t) => m.DiffuseTexture = t,
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

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
            WithNormals = false,
            WithUvs = true,
        },
        (m, t) => m.EmissiveTexture = t,
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

          uniform sampler2D emissiveTexture;

          out vec4 fragColor;

          in vec2 uv0;

          void main() {
            fragColor = vec4(1);
          
            vec4 emissiveColor = texture(emissiveTexture, uv0);
            fragColor.rgb += emissiveColor.rgb;
            fragColor.rgb = min(fragColor.rgb, 1.0);
          }
          """);

  [Test]
  public void TestWithNormalsNoTextures()
    => AssertGlsl_(
        new MockMaterialOptions {
            WithNormals = true,
        },
        (m, t) => { },
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

          {{GlslUtil.LIGHT_HEADER}}

          uniform bool hasSpecular;
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
          }
          """);

  [Test]
  public void TestWithNormalsDiffuseOnly()
    => AssertGlsl_(
        new MockMaterialOptions {
            TransparencyType = TransparencyType.MASK,
            WithNormals = true,
            WithUvs = true,
        },
        (m, t) => m.DiffuseTexture = t,
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

          {{GlslUtil.LIGHT_HEADER}}

          uniform sampler2D diffuseTexture;
          uniform bool hasSpecular;
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
            WithNormals = true,
            WithUvs = true,
        },
        (m, t) => m.EmissiveTexture = t,
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

          {{GlslUtil.LIGHT_HEADER}}

          uniform sampler2D emissiveTexture;
          uniform bool hasSpecular;
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
                              FinImage.Create1x1FromColor(options.TransparencyType switch {
                                  TransparencyType.OPAQUE      => Color.White,
                                  TransparencyType.MASK        => Color.FromArgb(0, 255, 255, 255),
                                  TransparencyType.TRANSPARENT => Color.FromArgb(128, 255, 255, 255),
                                  _                            => throw new ArgumentOutOfRangeException()
                              }));
                          var material = mm.AddStandardMaterial();
                          createMaterial(material, texture);
                          return material;
                        })
                    .FragmentShaderSource);
}