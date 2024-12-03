using System;
using System.Drawing;

using fin.image;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.util.image;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl;

public class StandardShaderSourceGlslTests {
  [Test]
  public void TestWithoutNormalsNoMask()
    => AssertGlsl_(
        false,
        false,
        (m, t) => { },
        """
        #version 430

        out vec4 fragColor;

        in vec4 vertexColor0;

        void main() {
          fragColor = vertexColor0;
        }
        """);

  [Test]
  public void TestWithoutNormalsNoTextures()
    => AssertGlsl_(
        false,
        true,
        (m, t) => { },
        """
        #version 430

        out vec4 fragColor;

        in vec4 vertexColor0;

        void main() {
          fragColor = vertexColor0;
        
          if (fragColor.a < .95) {
            discard;
          }
        }
        """);

  [Test]
  public void TestWithoutNormalsDiffuseOnly()
    => AssertGlsl_(
        false,
        true,
        (m, t) => m.DiffuseTexture = t,
        """
        #version 430

        uniform sampler2D diffuseTexture;

        out vec4 fragColor;

        in vec4 vertexColor0;
        in vec2 uv0;

        void main() {
          vec4 diffuseColor = texture(diffuseTexture, uv0);
          fragColor = diffuseColor * vertexColor0;
        
          if (fragColor.a < .95) {
            discard;
          }
        }
        """);

  [Test]
  public void TestWithoutNormalsEmissiveOnly()
    => AssertGlsl_(
        false,
        true,
        (m, t) => m.EmissiveTexture = t,
        """
        #version 430

        uniform sampler2D emissiveTexture;

        out vec4 fragColor;

        in vec4 vertexColor0;
        in vec2 uv0;

        void main() {
          fragColor = vertexColor0;
        
          vec4 emissiveColor = texture(emissiveTexture, uv0);
          fragColor.rgb += emissiveColor.rgb;
          fragColor.rgb = min(fragColor.rgb, 1);
        
          if (fragColor.a < .95) {
            discard;
          }
        }
        """);

  [Test]
  public void TestWithNormalsNoTextures()
    => AssertGlsl_(
        true,
        true,
        (m, t) => { },
        $$"""
          #version 430

          {{GlslUtil.GetLightHeader(true)}}

          uniform float shininess;

          out vec4 fragColor;

          in vec4 vertexColor0;
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          in vec3 tangent;
          in vec3 binormal;

          {{GlslUtil.GetGetIndividualLightColorsFunction()}}

          {{GlslUtil.GetGetMergedLightColorsFunction()}}

          {{GlslUtil.GetApplyMergedLightColorsFunction(false)}}

          void main() {
            fragColor = vertexColor0;
          
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
        true,
        true,
        (m, t) => m.DiffuseTexture = t,
        $$"""
          #version 430

          {{GlslUtil.GetLightHeader(true)}}

          uniform sampler2D diffuseTexture;
          uniform float shininess;

          out vec4 fragColor;

          in vec4 vertexColor0;
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          in vec3 tangent;
          in vec3 binormal;
          in vec2 uv0;

          {{GlslUtil.GetGetIndividualLightColorsFunction()}}

          {{GlslUtil.GetGetMergedLightColorsFunction()}}

          {{GlslUtil.GetApplyMergedLightColorsFunction(false)}}

          void main() {
            vec4 diffuseColor = texture(diffuseTexture, uv0);
            fragColor = diffuseColor * vertexColor0;
          
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
        true,
        true,
        (m, t) => m.EmissiveTexture = t,
        $$"""
          #version 430

          {{GlslUtil.GetLightHeader(true)}}

          uniform sampler2D emissiveTexture;
          uniform float shininess;

          out vec4 fragColor;

          in vec4 vertexColor0;
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          in vec3 tangent;
          in vec3 binormal;
          in vec2 uv0;

          {{GlslUtil.GetGetIndividualLightColorsFunction()}}

          {{GlslUtil.GetGetMergedLightColorsFunction()}}

          {{GlslUtil.GetApplyMergedLightColorsFunction(false)}}

          void main() {
            fragColor = vertexColor0;
          
            // Have to renormalize because the vertex normals can become distorted when interpolated.
            vec3 fragNormal = normalize(vertexNormal);
            fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, shininess, fragColor, vec4(1)).rgb, useLighting);
          
            vec4 emissiveColor = texture(emissiveTexture, uv0);
            fragColor.rgb += emissiveColor.rgb;
            fragColor.rgb = min(fragColor.rgb, 1);
          
            if (fragColor.a < .95) {
              discard;
            }
          }
          """);

  private static void AssertGlsl_(
      bool withNormals,
      bool masked,
      Action<IStandardMaterial, IReadOnlyTexture> createMaterial,
      string expectedSource) {
    var model = ModelImpl.CreateForViewer();

    var materialManager = model.MaterialManager;
    var material = materialManager.AddStandardMaterial();
    var texture = materialManager.CreateTexture(
        FinImage.Create1x1FromColor(Color.White));
    createMaterial(material, texture);

    if (withNormals) {
      var skin = model.Skin;

      var v = skin.AddVertex(0, 0, 0);
      v.SetLocalNormal(0, 0, 1);

      skin.AddMesh().AddPoints(v).SetMaterial(material);
    }

    material.TransparencyType
        = masked ? TransparencyType.MASK : TransparencyType.OPAQUE;

    var actualSource = new StandardShaderSourceGlsl(
        model,
        material,
        true,
        ShaderRequirements.FromModelAndMaterial(
            model,
            material)).FragmentShaderSource;

    Assert.AreEqual(expectedSource, actualSource);
  }
}