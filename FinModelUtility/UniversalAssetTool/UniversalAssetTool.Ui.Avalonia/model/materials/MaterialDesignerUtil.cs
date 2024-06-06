using System;
using System.Drawing;

using fin.image;
using fin.image.formats;
using fin.model;
using fin.model.impl;

using SixLabors.ImageSharp.PixelFormats;

namespace uni.ui.avalonia.model.materials;

public static class MaterialDesignerUtil {
  public static IReadOnlyMaterial CreateStubMaterial()
    => CreateStubModelAndMaterial().Item2;

  public static (IReadOnlyModel, IReadOnlyMaterial)
      CreateStubModelAndMaterial() {
    var model = new ModelImpl();
    var materialManager = model.MaterialManager;
    var material = materialManager.AddStandardMaterial();

    {
      var diffuseTexture
          = materialManager.CreateTexture(CreateStubImage(32, 32));
      diffuseTexture.Name = "Diffuse (Stub 1)";
      material.DiffuseTexture = diffuseTexture;
    }

    {
      var normalTexture
          = materialManager.CreateTexture(CreateStubImage(32, 64));
      normalTexture.Name = "Normal (Stub 2)";
      material.NormalTexture = normalTexture;
    }

    {
      var aoTexture = materialManager.CreateTexture(CreateStubImage(64, 32));
      aoTexture.Name = "Ambient occlusion (Stub 3)";
      material.AmbientOcclusionTexture = aoTexture;
    }

    {
      var emissiveTexture = materialManager.CreateTexture(
          FinImage.Create1x1FromColor(Color.Orange));
      emissiveTexture.Name = "Emissive (Orange)";
      material.EmissiveTexture = emissiveTexture;
    }

    {
      var specularTexture = materialManager.CreateTexture(
          FinImage.Create1x1FromColor(Color.Red));
      specularTexture.Name = "Specular (Red)";
      material.SpecularTexture = specularTexture;
    }

    return (model, material);
  }


  public static IReadOnlyTexture CreateStubTexture(int width, int height) {
    var model = new ModelImpl();
    var materialManager = model.MaterialManager;
    return materialManager.CreateTexture(CreateStubImage(width, height));
  }

  public static IReadOnlyImage CreateStubImage(int width, int height) {
    var image = new Rgba32Image(PixelFormat.ETC1, width, height);
    using var imgLock = image.Lock();
    var dst = imgLock.Pixels;

    var alphaPerPixel = 255 / MathF.Sqrt(width * width + height * height);

    for (var y = 0; y < height; ++y) {
      var yF = 1f * y / height;
      var nYF = 1 - yF;

      for (var x = 0; x < width; ++x) {
        var xF = 1f * x / width;
        var nXF = 1 - xF;

        var r = (byte) (255 * xF * yF);
        var g = (byte) (255 * nXF * yF);
        var b = (byte) (255 * nXF * nYF);
        var a = (byte) (alphaPerPixel + (255 - alphaPerPixel) * MathF.Pow(nXF * yF, .25f));

        
        dst[y * width + x] = new Rgba32(r, g, b, a);
      }
    }

    return image;
  }
}