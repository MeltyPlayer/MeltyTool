using System.Drawing;

using fin.image;
using fin.model.impl;
using fin.model.util;
using fin.util.image;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl;

public class TextureShaderSourceGlslTests {
  [Test]
  public void TestWithoutNormalsNotMasked()
    => AssertGlsl_(
        false,
        false,
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}
          
          uniform sampler2D diffuseTexture;
          
          out vec4 fragColor;
          
          in vec4 vertexColor0;
          in vec2 uv0;
          
          void main() {
            fragColor = texture(diffuseTexture, uv0) * vertexColor0;
          }
          """);

  [Test]
  public void TestWithoutNormalsMasked()
    => AssertGlsl_(
        false,
        true,
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
        true,
        true,
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}
          
          {{GlslUtil.GetLightHeader(true)}}

          uniform sampler2D diffuseTexture;
          uniform float shininess;

          out vec4 fragColor;

          in vec4 vertexColor0;
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          in vec2 uv0;

          {{GlslUtil.GetGetIndividualLightColorsFunction()}}

          {{GlslUtil.GetGetMergedLightColorsFunction()}}

          {{GlslUtil.GetApplyMergedLightColorsFunction(false)}}

          void main() {
            fragColor = texture(diffuseTexture, uv0) * vertexColor0;
          
            // Have to renormalize because the vertex normals can become distorted when interpolated.
            vec3 fragNormal = normalize(vertexNormal);
            fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, shininess, fragColor, vec4(1)).rgb,  useLighting);
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  private static void AssertGlsl_(
      bool withNormals,
      bool masked,
      string expectedSource) {
    var model = ModelImpl.CreateForViewer();

    var materialManager = model.MaterialManager;
    var material = materialManager.AddTextureMaterial(
        materialManager.CreateTexture(FinImage.Create1x1FromColor(Color.Red)));

    if (withNormals) {
      var skin = model.Skin;

      var v = skin.AddVertex(0, 0, 0);
      v.SetLocalNormal(0, 0, 1);

      skin.AddMesh().AddPoints(v).SetMaterial(material);
    }

    material.TransparencyType
        = masked ? TransparencyType.MASK : TransparencyType.OPAQUE;

    var actualSource = new TextureShaderSourceGlsl(
        model,
        material,
        true,
        ShaderRequirements.FromModelAndMaterial(
            model,
            material)).FragmentShaderSource;

    Assert.AreEqual(expectedSource, actualSource);
  }
}