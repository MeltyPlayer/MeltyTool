using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl.source;

public class NullShaderSourceGlslTests {
  [Test]
  [TestCase(false, false, false)]
  [TestCase(false, false, true)]
  [TestCase(false, true, false)]
  [TestCase(false, true, true)]
  [TestCase(true, false, false)]
  [TestCase(true, false, true)]
  [TestCase(true, true, false)]
  [TestCase(true, true, true)]
  public void TestWithoutColors(bool withNormals, bool withUvs, bool withMask)
    => AssertGlsl_(
        new MockMaterialOptions {
            WithColors = false,
            Masked = withMask,
            WithNormals = withNormals,
            WithUvs = withUvs,
        },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          out vec4 fragColor;

          void main() {
            fragColor = vec4(1);
          }
          """);

  [Test]
  [TestCase(false, false, false)]
  [TestCase(false, false, true)]
  [TestCase(false, true, false)]
  [TestCase(false, true, true)]
  [TestCase(true, false, false)]
  [TestCase(true, false, true)]
  [TestCase(true, true, false)]
  [TestCase(true, true, true)]
  public void TestWithColors(bool withNormals, bool withUvs, bool withMask)
    => AssertGlsl_(
        new MockMaterialOptions {
            WithColors = true,
            Masked = withMask,
            WithNormals = withNormals,
            WithUvs = withUvs,
        },
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          out vec4 fragColor;

          in vec4 vertexColor0;

          void main() {
            fragColor = vertexColor0;
          }
          """);

  private static void AssertGlsl_(MockMaterialOptions options,
                                  string expectedSource)
    => Assert.AreEqual(
        expectedSource,
        MockMaterial.BuildAndGetSource(
                        options,
                        mm => mm.AddNullMaterial())
                    .FragmentShaderSource);
}