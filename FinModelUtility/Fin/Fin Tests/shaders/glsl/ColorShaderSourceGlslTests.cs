using System.Drawing;

using fin.model.impl;
using fin.model.util;
using fin.util.image;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl;

public class ColorShaderSourceGlslTests {
  [Test]
  public void TestWithoutNormalsNotMasked()
    => AssertGlsl_(
        false,
        false,
        """
        #version 430

        uniform vec4 diffuseColor;
        
        out vec4 fragColor;
        
        in vec4 vertexColor0;

        void main() {
          fragColor = diffuseColor * vertexColor0;
        }
        """);

  [Test]
  public void TestWithoutNormalsMasked()
    => AssertGlsl_(
        false,
        true,
        """
        #version 430

        uniform vec4 diffuseColor;

        out vec4 fragColor;

        in vec4 vertexColor0;

        void main() {
          fragColor = diffuseColor * vertexColor0;
        
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
          #version 430

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

  private static void AssertGlsl_(
      bool withNormals,
      bool masked,
      string expectedSource) {
    var model = ModelImpl.CreateForViewer();

    var materialManager = model.MaterialManager;
    var material = materialManager.AddColorMaterial(Color.Red);

    if (withNormals) {
      var skin = model.Skin;

      var v = skin.AddVertex(0, 0, 0);
      v.SetLocalNormal(0, 0, 1);

      skin.AddMesh().AddPoints(v).SetMaterial(material);
    }

    material.TransparencyType
        = masked ? TransparencyType.MASK : TransparencyType.OPAQUE;

    var actualSource = new ColorShaderSourceGlsl(
        model,
        material,
        true,
        ShaderRequirements.FromModelAndMaterial(
            model,
            material)).FragmentShaderSource;

    Assert.AreEqual(expectedSource, actualSource);
  }
}