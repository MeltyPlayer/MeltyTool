using System;
using System.Drawing;

using fin.image;
using fin.image.formats;
using fin.model;
using fin.model.impl;

using SixLabors.ImageSharp.PixelFormats;

namespace uni.ui.avalonia.model;

public static class ModelDesignerUtil {
  public static IReadOnlyModel CreateStubModel() {
    var model = new ModelImpl();

    {
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
    }

    {
      var skeleton = model.Skeleton;
      
      var rootBone = skeleton.Root;

      var center = rootBone.AddChild(0, 0, 0);
      center.Name = "center";

      var leg = center.AddChild(0, 0, 0);
      leg.Name = "leg";

      var foot = leg.AddChild(0, 0, 0);
      foot.Name = "foot";

      var arm = center.AddChild(0, 0, 0);
      arm.Name = "arm";

      var hand = arm.AddChild(0, 0, 0);
      hand.Name = "hand";

      var unnamed = center.AddChild(0, 0, 0);
    }

    {
      var animationManager = model.AnimationManager;

      var multiFrameAnimation = animationManager.AddAnimation();
      multiFrameAnimation.Name = "multi frame animation";
      multiFrameAnimation.FrameRate = 24;
      multiFrameAnimation.FrameCount = 125;

      var singleFrameAnimation = animationManager.AddAnimation();
      singleFrameAnimation.Name = "single frame animation";
      singleFrameAnimation.FrameRate = 30;
      singleFrameAnimation.FrameCount = 1;

      var emptyAnimation = animationManager.AddAnimation();
      emptyAnimation.Name = "empty animation";
      emptyAnimation.FrameRate = 60;
      emptyAnimation.FrameCount = 0;
    }

    return model;
  }

  public static IReadOnlyAnimation CreateStubAnimation()
    => CreateStubModel().AnimationManager.Animations[0];

  public static IReadOnlyMaterial CreateStubMaterial()
    => CreateStubModelAndMaterial().material;

  public static (IReadOnlyModel model, IReadOnlyMaterial material)
      CreateStubModelAndMaterial() {
    var model = CreateStubModel();
    var material = model.MaterialManager.All[0];
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
        var a = (byte) (alphaPerPixel +
                        (255 - alphaPerPixel) * MathF.Pow(nXF * yF, .25f));


        dst[y * width + x] = new Rgba32(r, g, b, a);
      }
    }

    return image;
  }
}