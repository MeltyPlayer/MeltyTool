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
    while (boneQueue.TryDequeue(out var skelBone, out var parentFinBone)) {
      var offset = skelBone.Offset;
      var finBone = (parentFinBone ?? finRootBone).AddChild(
          offset.X,
          offset.Y,
          offset.Z);
      finBone.Name = skelBone.Name;

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

      var finBonesFromSkel = finModel.Skeleton.Bones.Skip(1).ToArray();
      for (var i = 0; i < finBonesFromSkel.Length; ++i) {
        var boneEulerRotations = anim.BoneEulerRotations[i];
        var finBoneTracks
            = finAnimation.GetOrCreateBoneTracks(finBonesFromSkel[i]);

        var rotationTrack = finBoneTracks.UseCombinedQuaternionKeyframes();
        rotationTrack.SetAllKeyframes(
            boneEulerRotations.Select(
                eulerRotation => eulerRotation.CreateZyxRadians()));
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
        files);

    return finModel;
  }
}