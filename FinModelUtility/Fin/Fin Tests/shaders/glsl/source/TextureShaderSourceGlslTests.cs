using System;
using System.Drawing;

using fin.image;
using fin.image.util;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl.source;

public sealed class TextureShaderSourceGlslTests {
  [Test]
  public void TestWithoutNormalsNotMasked()
    => AssertGlsl_(
        new MockMaterialOptions {
            TransparencyType = TransparencyType.TRANSPARENT,
            WithUvs = true,
        },
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

          uniform sampler2D diffuseTexture;

          out vec4 fragColor;

          in vec2 uv0;

          void main() {
            fragColor = texture(diffuseTexture, uv0);
          
            if (fragColor.a < .01) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithColorsWithoutNormalsMasked()
    => AssertGlsl_(
        new MockMaterialOptions {
            TransparencyType = TransparencyType.MASK,
            WithNormals = false,
            WithColors = true,
            WithUvs = true,
        },
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITH_TEXTURES}}

          uniform sampler2D diffuseTexture;

          out vec4 fragColor;

          in vec4 vertexColor0;
          in vec2 uv0;

          void main() {
            fragColor = texture(diffuseTexture, uv0) * vertexColor0;
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithNormals()
    => AssertGlsl_(
        new MockMaterialOptions {
            TransparencyType = TransparencyType.MASK,
            WithNormals = true,
            WithUvs = true,
        },
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
            fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, shininess, fragColor, vec4(1)).rgb,  useLighting);
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  private static void AssertGlsl_(
      MockMaterialOptions options,
      string expectedSource)
    => Assert.AreEqual(
        expectedSource,
        MockMaterial.BuildAndGetSource(
                        options,
                        mm => mm.AddTextureMaterial(
                            mm.CreateTexture(
                                FinImage.Create1x1FromColor(
                                    options.TransparencyType switch {
                                        TransparencyType.OPAQUE      => Color.White,
                                        TransparencyType.MASK        => Color.FromArgb(0, 255, 255, 255),
                                        TransparencyType.TRANSPARENT => Color.FromArgb(128, 255, 255, 255),
                                        _                            => throw new ArgumentOutOfRangeException()
                                    }))))
                    .FragmentShaderSource);
}