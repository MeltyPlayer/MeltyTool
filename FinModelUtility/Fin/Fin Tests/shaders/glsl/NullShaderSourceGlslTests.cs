using System.Drawing;

using fin.image;
using fin.model.impl;
using fin.model.util;
using fin.util.image;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.shaders.glsl;

public class NullShaderSourceGlslTests {
  [Test]
  public void TestWithoutNormalsNotMasked()
    => AssertGlsl_(
        false,
        false,
        """
        #version 430
        
        out vec4 fragColor;
        
        in vec4 vertexColor0;
        
        void main() {
          fragColor = vertexColor0;
        }
        """);

  [Test]
  public void TestWithoutNormalsMasked()
    => AssertGlsl_(
        false,
        true,
        """
        #version 430
        
        out vec4 fragColor;
        
        in vec4 vertexColor0;
        
        void main() {
          fragColor = vertexColor0;
        }
        """);

  [Test]
  public void TestWithNormals()
    => AssertGlsl_(
        true,
        true,
        """
        #version 430

        out vec4 fragColor;

        in vec4 vertexColor0;

        void main() {
          fragColor = vertexColor0;
        }
        """);

  private static void AssertGlsl_(
      bool withNormals,
      bool masked,
      string expectedSource) {
    var model = ModelImpl.CreateForViewer();

    var materialManager = model.MaterialManager;
    var material = materialManager.AddNullMaterial();

    if (withNormals) {
      var skin = model.Skin;

      var v = skin.AddVertex(0, 0, 0);
      v.SetLocalNormal(0, 0, 1);

      skin.AddMesh().AddPoints(v).SetMaterial(material);
    }

    material.TransparencyType
        = masked ? TransparencyType.MASK : TransparencyType.OPAQUE;

    var actualSource = new NullShaderSourceGlsl(
        model,
        true,
        ShaderRequirements.FromModelAndMaterial(
            model,
            material)).FragmentShaderSource;

    Assert.AreEqual(expectedSource, actualSource);
  }
}