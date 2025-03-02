using System.Numerics;

using fin.animation.keyframes;
using fin.data.queues;
using fin.io;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.util.enumerables;
using fin.util.sets;

using xmod.schema.anim;
using xmod.schema.ped;
using xmod.schema.skel;


namespace xmod.api;

public class PedModelImporter : IModelImporter<PedModelFileBundle> {
  public IModel Import(PedModelFileBundle modelFileBundle) {
    var pedFile = modelFileBundle.PedFile;
    var ped = pedFile.ReadNewFromText<Ped>();

    var files = pedFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = files
    };

    var animDirectory = pedFile.AssertGetParent();
    var skelFile = animDirectory.AssertGetExistingFile($"{ped.SkelName}.skel");
    var skel = skelFile.ReadNewFromText<Skel>();

    var finRootBone = finModel.Skeleton.Root;
    var boneQueue = new FinTuple2Queue<SkelBone, IBone?>((skel.Root, null));
    var finBoneById = new Dictionary<int, IReadOnlyBone>();
    while (boneQueue.TryDequeue(out var skelBone, out var parentFinBone)) {
      var offset = skelBone.Offset;
      var finBone = (parentFinBone ?? finRootBone).AddChild(
          offset.X,
          offset.Y,
          offset.Z);
      finBone.Name = skelBone.Name;
      finBoneById[skelBone.Id] = finBone;

      boneQueue.Enqueue(skelBone.Children.Select(child => (child, finBone)));
    }

    var finAnimationManager = finModel.AnimationManager;
    foreach (var (animName, animFileName) in ped.AnimMap) {
      if (!animDirectory.TryToGetExistingFile($"{animFileName}.anim",
                                              out var animFile)) {
        continue;
      }

      var anim = animFile.ReadNew<Anim>();

      var finAnimation = finAnimationManager.AddAnimation();
      finAnimation.Name = animName;

      finAnimation.FrameCount = anim.FrameCount;
      finAnimation.FrameRate = 30;

      var rootBoneTracks = finAnimation.GetOrCreateBoneTracks(finRootBone);
      rootBoneTracks.UseCombinedTranslationKeyframes()
                    .SetAllKeyframes(anim.RootPositions);

      for (var i = 0; i < anim.BoneEulerRotations.Count; ++i) {
        var boneEulerRotations = anim.BoneEulerRotations[i];
        var finBoneTracks = finAnimation.GetOrCreateBoneTracks(finBoneById[i]);

        var rotationTrack = finBoneTracks.UseSeparateEulerRadiansKeyframes();
        for (var f = 0; f < anim.FrameCount; ++f) {
          var eulerRotation = boneEulerRotations[f];

          rotationTrack.Axes[0].SetKeyframe(f, eulerRotation.X);
          rotationTrack.Axes[1].SetKeyframe(f, eulerRotation.Y);
          rotationTrack.Axes[2].SetKeyframe(f, eulerRotation.Z);
        }
      }
    }

    var xmodFile = modelFileBundle.ModelDirectory.AssertGetExistingFile(
        $"{ped.XmodNames[0]}.xmod");
    new XmodModelImporter().ImportInto(
        new XmodModelFileBundle {
            XmodFile = xmodFile,
            TextureDirectory = modelFileBundle.TextureDirectory,
        },
        finModel,
        files,
        finBoneById);

    return finModel;
  }
}