using System.Drawing;

using fin.image.util;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl.source;

public sealed class ColorShaderSourceGlslTests {
  [Test]
  public void TestWithoutNormalsTransparent()
    => AssertGlsl_(
        new MockMaterialOptions {
            TransparencyType = TransparencyType.TRANSPARENT,
            WithNormals = false,
        },
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITHOUT_TEXTURES}}

          uniform vec4 diffuseColor;

          out vec4 fragColor;

          void main() {
            fragColor = diffuseColor;
          
            if (fragColor.a < .01) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithoutNormalsOpaque()
    => AssertGlsl_(
        new MockMaterialOptions {
            WithNormals = false,
        },
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITHOUT_TEXTURES}}

          uniform vec4 diffuseColor;

          out vec4 fragColor;

          void main() {
            fragColor = diffuseColor;
          }
          """);

  [Test]
  public void TestWithVertexColorAndNormal()
    => AssertGlsl_(
        new MockMaterialOptions {
            WithNormals = true,
            WithColors = true,
        },
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITHOUT_TEXTURES}}

          {{GlslUtil.LIGHT_HEADER}}

          uniform vec4 diffuseColor;
          uniform bool hasSpecular;
          uniform float shininess;

          out vec4 fragColor;

          in vec4 vertexColor0;
          in vec3 vertexPosition;
          in vec3 vertexNormal;

          {{GlslUtil.GetGetIndividualLightColorsFunction()}}

          {{GlslUtil.GetGetMergedLightColorsFunction()}}

          {{GlslUtil.GetApplyMergedLightColorsFunction(false)}}

          void main() {
            fragColor = diffuseColor * vertexColor0;
          
            // Have to renormalize because the vertex normals can become distorted when interpolated.
            vec3 fragNormal = normalize(vertexNormal);
            fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, shininess, fragColor, vec4(1)).rgb,  useLighting);
          }
          """);

  private static void AssertGlsl_(MockMaterialOptions options,
                                  string expectedSource)
    => Assert.AreEqual(
        expectedSource,
        MockMaterial.BuildAndGetSource(
                        options,
                        mm => mm.AddColorMaterial(
                            options.TransparencyType is not TransparencyType
                                .TRANSPARENT
                                ? Color.Red
                                : Color.FromArgb(128, 255, 0, 0)))
                    .FragmentShaderSource);
}