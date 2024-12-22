using System.Drawing;

using fin.animation.keyframes;
using fin.compression;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.dictionaries;
using fin.util.sets;

using schema.binary;

using sm64ds.schema.bca;
using sm64ds.schema.bmd;

namespace sm64ds.api;

public class Sm64dsModelImporter : IModelImporter<Sm64dsModelFileBundle> {
  public IModel Import(Sm64dsModelFileBundle fileBundle) {
    var bmdFile = fileBundle.BmdFile;

    var files = bmdFile.AsFileSet();
    var model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    using var bmdRawBr = bmdFile.OpenReadAsBinary();
    var bmdBr
        = new SchemaBinaryReader(new Lz77Decompressor().Decompress(bmdRawBr));
    var bmd = bmdBr.ReadNew<Bmd>();

    // Set up bones
    var finBones = new IReadOnlyBone[bmd.Bones.Length];
    {
      var bones = bmd.Bones.OrderBy(b => b.Id).ToArray();
      var rootBones = new List<Bone>();
      var boneToChildMap = new SetDictionary<Bone, Bone>();
      var nextSiblingMap = new Dictionary<Bone, Bone>();
      foreach (var bone in bones) {
        var offsetToParentBone = bone.OffsetToParentBone;
        if (offsetToParentBone != 0) {
          boneToChildMap.Add(bones[bone.Id + offsetToParentBone], bone);
        } else {
          rootBones.Add(bone);
        }

        var offsetToNextSibling = bone.OffsetToNextSiblingBone;
        if (offsetToNextSibling != 0) {
          nextSiblingMap[bone] = bones[bone.Id + offsetToNextSibling];
        }
      }

      var previousSiblingMap = nextSiblingMap.SwapKeysAndValues();

      var boneQueue = new FinTuple2Queue<Bone, IBone>(
          rootBones.Select(b => (b, model.Skeleton.Root)));
      while (boneQueue.TryDequeue(out var bone, out var parentFinBone)) {
        var finBone = parentFinBone.AddChild(bone.Translation);
        finBone.Name = bone.Name;

        finBones[bone.Id] = finBone;

        var localTransform = finBone.LocalTransform;
        localTransform.SetRotationDegrees(bone.Rotation);
        localTransform.SetScale(bone.Scale);

        if (boneToChildMap.TryGetSet(bone, out var unorderedChildren)) {
          var firstChild
              = unorderedChildren!.Single(
                  b => !previousSiblingMap.ContainsKey(b));
          boneQueue.Enqueue(nextSiblingMap.Chain(firstChild)
                                          .Select(b => (b, finBone)));
        }
      }
    }

    // Set up materials
    var finMaterialManager = model.MaterialManager;
    var lazyTextureDictionary
        = new LazyDictionary<(Texture texture, Palette? palette), ITexture>(
            textureAndPalette => {
              var (sm64Texture, sm64Palette) = textureAndPalette;

              var finTexture
                  = finMaterialManager.CreateTexture(
                      ImageReader.ReadImage(sm64Texture, sm64Palette));
              finTexture.Name = sm64Texture.Name;

              return finTexture;
            });

    foreach (var sm64Material in bmd.Materials) {
      var textureId = sm64Material.TextureId;
      var paletteId = sm64Material.TexturePaletteId;

      if (textureId != -1) {
        var sm64Texture = bmd.Textures[textureId];
        var sm64Palette = paletteId != -1 ? bmd.Palettes[paletteId] : null;

        var finTexture = lazyTextureDictionary[(sm64Texture, sm64Palette)];
      }
    }

    // Set up mesh
    { }

    // Set up animations
    if (fileBundle.BcaFiles != null) {
      foreach (var bcaFile in fileBundle.BcaFiles) {
        using var bcaRawBr = bcaFile.OpenReadAsBinary();
        var bcaBr = new SchemaBinaryReader(
            new Lz77Decompressor().Decompress(bcaRawBr));
        var bca = bcaBr.ReadNew<Bca>();

        var finAnimation = model.AnimationManager.AddAnimation();
        finAnimation.Name = bcaFile.NameWithoutExtension.ToString();

        finAnimation.FrameRate = 30;
        finAnimation.FrameCount = bca.NumFrames;
        finAnimation.UseLoopingInterpolation = bca.Looped;

        for (var i = 0; i < bca.BoneAnimationData.Length; ++i) {
          var boneAnimationData = bca.BoneAnimationData[i];

          var boneTracks = finAnimation.AddBoneTracks(finBones[i]);

          (int, float)[][] translationAxes = [
              boneAnimationData.TranslationXValues,
              boneAnimationData.TranslationYValues,
              boneAnimationData.TranslationZValues
          ];
          var translations = boneTracks.UseSeparateTranslationKeyframes();
          for (var a = 0; a < translationAxes.Length; ++a) {
            var translationAxis = translationAxes[a];
            foreach (var (f, value) in translationAxis) {
              translations.SetKeyframe(a, f, value);
            }
          }

          (int, float)[][] rotationAxes = [
              boneAnimationData.RotationXValues,
              boneAnimationData.RotationYValues,
              boneAnimationData.RotationZValues
          ];
          var rotations = boneTracks.UseSeparateEulerRadiansKeyframes();
          for (var a = 0; a < rotationAxes.Length; ++a) {
            var rotationAxis = rotationAxes[a];
            foreach (var (f, value) in rotationAxis) {
              rotations.SetKeyframe(a, f, value);
            }
          }

          (int, float)[][] scaleAxes = [
              boneAnimationData.ScaleXValues,
              boneAnimationData.ScaleYValues,
              boneAnimationData.ScaleZValues
          ];
          var scales = boneTracks.UseSeparateScaleKeyframes();
          for (var a = 0; a < scaleAxes.Length; ++a) {
            var scaleAxis = scaleAxes[a];
            foreach (var (f, value) in scaleAxis) {
              scales.SetKeyframe(a, f, value);
            }
          }
        }
      }
    }

    return model;
  }
}