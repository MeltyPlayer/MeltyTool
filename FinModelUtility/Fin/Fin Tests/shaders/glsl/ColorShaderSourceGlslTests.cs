using System.Drawing;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl;

public class ColorShaderSourceGlslTests {
  [Test]
  public void TestWithoutNormalsNotMasked()
    => AssertGlsl_(
        new MockMaterialOptions {
            Masked = false,
            WithNormals = false,
        },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          uniform vec4 diffuseColor;

          out vec4 fragColor;

          void main() {
            fragColor = diffuseColor;
          }
          """);

  [Test]
  public void TestWithoutNormalsMasked()
    => AssertGlsl_(
        new MockMaterialOptions {
            Masked = true,
            WithNormals = false,
        },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          uniform vec4 diffuseColor;

          out vec4 fragColor;

          void main() {
            fragColor = diffuseColor;
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  [Test]
  public void TestWithVertexColorAndNormal()
    => AssertGlsl_(
        new MockMaterialOptions {
            Masked = true,
            WithNormals = true,
            WithColors = true,
        },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          {{GlslUtil.GetLightHeader(true)}}

          uniform vec4 diffuseColor;
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
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  private static void AssertGlsl_(MockMaterialOptions options,
                                  string expectedSource)
    => Assert.AreEqual(
        expectedSource,
        MockMaterial.BuildAndGetSource(
                        options,
                        mm => mm.AddColorMaterial(Color.Red))
                    .FragmentShaderSource);
}