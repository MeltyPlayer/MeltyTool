using System.Drawing;

using fin.image;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl.source;

public class TextureShaderSourceGlslTests {
  [Test]
  public void TestWithoutNormalsNotMasked()
    => AssertGlsl_(
        new MockMaterialOptions { WithUvs = true, },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

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
            Masked = true,
            WithNormals = false,
            WithColors = true,
            WithUvs = true,
        },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

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
            Masked = true,
            WithNormals = true,
            WithUvs = true,
        },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          {{GlslUtil.GetLightHeader(true)}}

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
                                FinImage.Create1x1FromColor(Color.Red))))
                    .FragmentShaderSource);
}