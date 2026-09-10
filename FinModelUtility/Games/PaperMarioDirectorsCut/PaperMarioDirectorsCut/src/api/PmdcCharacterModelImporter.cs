using System.Numerics;

using fin.animation.keyframes;
using fin.image;
using fin.io;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;


namespace pmdc.api;

public sealed class PmdcCharacterModelFileBundle : IModelFileBundle {
  public required IReadOnlyTreeFile[] AnimationImageFiles { get; init; }
  public required IReadOnlyTreeDirectory CharactersDirectory { get; init; }

  public IReadOnlyTreeFile MainFile => this.AnimationImageFiles[0];

  public ReadOnlySpan<char> DisplayFullPath
    => this.MainFile.AssertGetParent().FullPath;

  public IEnumerable<IReadOnlyGenericFile> Files => this.AnimationImageFiles;
}

public sealed class PmdcCharacterModelImporter
    : IModelImporter<PmdcCharacterModelFileBundle> {
  public IModel Import(PmdcCharacterModelFileBundle bundle) {
    var model = new ModelImpl() {
        FileBundle = bundle,
        Files = bundle.Files.ToHashSet(),
    };

    var backAnimationImageFile
        = bundle.AnimationImageFiles.SingleOrDefaultByName("e.gif");

    var orderedAnimationImageFiles
        = bundle.AnimationImageFiles.Where(f => !f.Name.Equals("e.gif", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.Name.Equals(
                                       "s.gif",
                                       StringComparison.OrdinalIgnoreCase));

    ITexture baseTexture = null!;
    ITextureMaterial material = null!;

    var finMaterialManager = model.MaterialManager;
    foreach (var animationImageFile in orderedAnimationImageFiles) {
      var frameImages = FinImage.FromGifFile(animationImageFile);
      var name = animationImageFile.NameWithoutExtension.ToString();

      var frameTextures
          = frameImages
            .Select((frameImage, i) => {
              var frameTexture = finMaterialManager.CreateTexture(
                  frameImage.RemoveTopLeftBackgroundColor());
              frameTexture.Name = frameImages.Length > 0 ? $"{name}_{i}" : name;
              frameTexture.WrapModeU = frameTexture.WrapModeV = WrapMode.CLAMP;
              return frameTexture;
            })
            .ToArray();

      if (material == null) {
        baseTexture = frameTextures[0];

        material = finMaterialManager.AddTextureMaterial(baseTexture);
        material.Name = "diffuse";
      }

      var animation = model.AnimationManager.AddAnimation();
      animation.Name = name;
      animation.FrameCount = frameImages.Length;
      animation.FrameRate = 30;

      var textureTracks = animation.AddTextureTracks(baseTexture);
      var flipbookSwapKeyframes
          = textureTracks.UseFlipbookSwapKeyframes(frameTextures.Length);
      for (var f = 0; f < frameTextures.Length; ++f) {
        flipbookSwapKeyframes.Add(new Keyframe<IReadOnlyTexture?>(f, frameTextures[f]));
      }
    }

    var rootBone = model.Skeleton.Root;

    var billboardBone = rootBone.AddChild(new Vector3(0, -1, 0));
    billboardBone.Name = "billboard";
    billboardBone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_ONLY,
                                          QuaternionUtil.CreateZyxRadians(
                                              -MathF.PI / 2,
                                              0,
                                              0));

    var (width, height) = (32f, 32f);

    var skin = model.Skin;
    var mesh = skin.AddMesh();
    mesh.Name = "mesh";

    {
      var ul = new Vector3(-width / 2, height, 0);
      var ur = new Vector3(width / 2, height, 0);
      var lr = new Vector3(width / 2, 0, 0);
      var ll = new Vector3(-width / 2, 0, 0);

      mesh.AddSimpleQuad(skin,
                         (ul, new Vector2(0, 0), null),
                         (ur, new Vector2(1, 0), null),
                         (lr, new Vector2(1, 1), null),
                         (ll, new Vector2(0, 1), null),
                         material,
                         billboardBone);
    }

    if (backAnimationImageFile != null) {
      var backFrameImages = FinImage.FromGifFile(backAnimationImageFile);
      var name = backAnimationImageFile.NameWithoutExtension.ToString();

      var backFrameTextures
          = backFrameImages
            .Select((frameImage, i) => {
              var frameTexture = finMaterialManager.CreateTexture(
                  frameImage.RemoveTopLeftBackgroundColor());
              frameTexture.Name = backFrameImages.Length > 0 ? $"{name}_{i}" : name;
              frameTexture.WrapModeU = frameTexture.WrapModeV = WrapMode.CLAMP;
              return frameTexture;
            })
            .ToArray();

      var backBaseTexture = backFrameTextures[0];

      var backMaterial = finMaterialManager.AddTextureMaterial(backBaseTexture);
      backMaterial.Name = "diffuse";

      var animation = model.AnimationManager.AddAnimation();
      animation.Name = name;
      animation.FrameCount = backFrameImages.Length;
      animation.FrameRate = 30;

      var textureTracks = animation.AddTextureTracks(backBaseTexture);
      var flipbookSwapKeyframes
          = textureTracks.UseFlipbookSwapKeyframes(backFrameTextures.Length);
      for (var f = 0; f < backFrameTextures.Length; ++f) {
        flipbookSwapKeyframes.Add(new Keyframe<IReadOnlyTexture?>(f, backFrameTextures[f]));
      }

      var backDistance = -1;

      var ul = new Vector3(-width / 2, height, backDistance);
      var ur = new Vector3(width / 2, height, backDistance);
      var lr = new Vector3(width / 2, 0, backDistance);
      var ll = new Vector3(-width / 2, 0, backDistance);

      mesh.AddSimpleQuad(skin,
                         (ul, new Vector2(0, 0), null),
                         (ur, new Vector2(1, 0), null),
                         (lr, new Vector2(1, 1), null),
                         (ll, new Vector2(0, 1), null),
                         backMaterial,
                         billboardBone);
    }

    {
      var shadowBone = rootBone.AddChild(Vector3.Zero);
      shadowBone.Name = "shadow";

      var (shadowMaterial, shadowTexture)
          = finMaterialManager.AddSimpleTextureMaterialFromFile(
              bundle.CharactersDirectory
                    .AssertGetExistingFile("bacShadow.png"));
      shadowTexture.WrapModeU = shadowTexture.WrapModeV = WrapMode.CLAMP;

      var ul = new Vector3(-width / 2, 0, -height / 2);
      var ur = new Vector3(width / 2, 0, -height / 2);
      var lr = new Vector3(width / 2, 0, height / 2);
      var ll = new Vector3(-width / 2, 0, height / 2);

      mesh.AddSimpleQuad(skin,
                         (ul, new Vector2(0, 0), null),
                         (ur, new Vector2(1, 0), null),
                         (lr, new Vector2(1, 1), null),
                         (ll, new Vector2(0, 1), null),
                         shadowMaterial,
                         shadowBone);
    }

    return model;
  }
}