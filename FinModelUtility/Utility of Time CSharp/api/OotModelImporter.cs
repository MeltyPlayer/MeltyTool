using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.animation.keyframes;
using fin.animation.types.quaternion;
using fin.data.queues;
using fin.model;
using fin.model.io.importers;
using fin.util.enumerables;

using UoT.hacks;
using UoT.memory;
using UoT.model;

namespace UoT.api {
  public enum OotSegmentIndex : uint {
    GAMEPLAY_KEEP = 4,
    GAMEPLAY_FIELD_KEEP = 5,
    ZOBJECT = 6,
    LINK_ANIMETION = 7,
  }

  public class OotModelImporter : IModelImporter<OotModelFileBundle> {
    public IModel Import(OotModelFileBundle modelFileBundle) {
      var zFile = modelFileBundle.ZFile;
      var isLink = zFile.FileName is "object_link_boy"
                                     or "object_link_child"
                                     or "object_torch2";

      var n64Memory = new N64Memory(modelFileBundle.OotRom);

      var n64Hardware = new N64Hardware<N64Memory>();
      n64Hardware.Memory = n64Memory;
      n64Hardware.Rdp = new Rdp { Tmem = new JankTmem(n64Hardware) };
      n64Hardware.Rsp = new Rsp();

      n64Hardware.Rsp.GeometryMode = (GeometryMode) 0x22405;

      var zSegments = ZSegments.Instance;

      var gameplayKeep =
          zSegments.Others.Single(other => other.FileName is "gameplay_keep");
      n64Memory.AddSegment((uint) OotSegmentIndex.GAMEPLAY_KEEP,
                           gameplayKeep.Segment);

      // TODO: Use "gameplay dangeon keep" when applicable
      var gameplayFieldKeep =
          zSegments.Others.Single(
              other => other.FileName is "gameplay_field_keep");
      n64Memory.AddSegment((uint) OotSegmentIndex.GAMEPLAY_FIELD_KEEP,
                           gameplayFieldKeep.Segment);

      n64Memory.AddSegment((uint) OotSegmentIndex.ZOBJECT,
                           zFile.Segment);

      var linkAnimetion =
          zSegments.Others.SingleOrDefault(
              other => other.FileName is "link_animetion");
      if (isLink) {
        n64Memory.AddSegment((uint) OotSegmentIndex.LINK_ANIMETION,
                             linkAnimetion.Segment);
      }

      Hacks.ApplyHacks(n64Hardware, modelFileBundle.ZFile.FileName);

      var dlModelBuilder = new DlModelBuilder(n64Hardware);
      var finModel = dlModelBuilder.Model;

      var ootLimbs =
          new LimbHierarchyReader2().GetHierarchies(n64Memory, isLink);
      if (ootLimbs != null) {
        var finBones = new IBone[ootLimbs.Count];
        var ootLimbQueue =
            new FinTuple2Queue<IBone, int>((finModel.Skeleton.Root, 0));
        while (ootLimbQueue.TryDequeue(out var parentFinBone,
                                       out var ootLimbIndex)) {
          var ootLimb = ootLimbs[ootLimbIndex];
          var finBone = parentFinBone.AddChild(ootLimb.X, ootLimb.Y, ootLimb.Z);
          finBones[ootLimbIndex] = finBone;

          // TODO: Handle DLs
          // TODO: Handle animations

          var firstChildIndex = ootLimb.FirstChildIndex;
          if (firstChildIndex != -1) {
            ootLimbQueue.Enqueue((finBone, firstChildIndex));
          }

          var nextSiblingIndex = ootLimb.NextSiblingIndex;
          if (nextSiblingIndex != -1) {
            ootLimbQueue.Enqueue((parentFinBone, nextSiblingIndex));
          }
        }

        var visibleFinBonesAndOotLimbs = new List<(IBone, ILimb2)>();
        for (var i = 0; i < finBones.Length; ++i) {
          var ootLimb = ootLimbs[i];
          IoUtils.SplitSegmentedAddress(ootLimb.DisplayListSegmentedAddress,
                                        out var dlSegmentIndex,
                                        out _);

          if (dlSegmentIndex == 0) {
            continue;
          }

          var finBone = finBones[i];

          n64Hardware.Rsp.BoneMapper.SetBoneAtSegmentedAddress(
              (uint) 0x0d000000 +
              (uint) (0x40 * visibleFinBonesAndOotLimbs.Count),
              finBone);
          visibleFinBonesAndOotLimbs.Add((finBone, ootLimb));
        }

        foreach (var (finBone, ootLimb) in visibleFinBonesAndOotLimbs) {
          n64Hardware.Rsp.ActiveBone = finBone;

          var displayList =
              new DisplayListReader().ReadDisplayList(
                  n64Memory,
                  new F3dzex2OpcodeParser(),
                  ootLimb.DisplayListSegmentedAddress);
          dlModelBuilder.AddDl(displayList);
        }

        var animationReader = new AnimationReader2();
        IList<IAnimation>? ootAnimations;
        if (isLink) {
          ootAnimations = animationReader.GetLinkAnimations(n64Memory,
            gameplayKeep,
            ootLimbs.Count);
        } else {
          var animationFiles
              = zSegments
                .Objects
                .Where(s => s.FileName.EndsWith("_anime") ||
                            s.FileName.EndsWith("_keep") ||
                            s.FileName.EndsWith("_animetion") ||
                            s.FileName.EndsWith("_anime1") ||
                            s.FileName.EndsWith("_anime2") ||
                            s.FileName.EndsWith("_anime3"))
                .Concat(zFile.Yield())
                .ToArray();

          ootAnimations = animationReader.GetCommonAnimations(
              n64Memory,
              animationFiles,
              ootLimbs.Count);
        }

        var animationIndex = 0;
        if (ootAnimations != null) {
          var rootBone = finBones[0];

          foreach (var ootAnimation in ootAnimations) {
            var finAnimation = finModel.AnimationManager.AddAnimation();
            finAnimation.FrameRate = 20;
            var frameCount = finAnimation.FrameCount = ootAnimation.FrameCount;

            finAnimation.Name = $"Animation {animationIndex++}";

            var rootAnimationTracks = finAnimation.AddBoneTracks(rootBone);
            var positions
                = rootAnimationTracks.UseCombinedTranslationKeyframes(
                    frameCount);
            for (var f = 0; f < frameCount; ++f) {
              var pos = ootAnimation.GetPosition(f);
              positions.SetKeyframe(f, new Vector3(pos.X, pos.Y, pos.Z));
            }

            for (var i = 0; i < ootLimbs.Count; ++i) {
              var finBone = finBones[i];
              var animationTracks = i == 0
                  ? rootAnimationTracks
                  : finAnimation.AddBoneTracks(finBone);
              var rotations
                  = animationTracks
                      .UseSeparateEulerRadiansKeyframes(frameCount);

              rotations.ConvertRadiansToQuaternionImpl =
                  ConvertRadiansToQuaternionOot_;

              for (var a = 0; a < 3; ++a) {
                this.AddOotAnimationTrackToFin_(
                    ootAnimation.GetTrack(i * 3 + a),
                    a,
                    rotations);
              }
            }
          }
        }
      }

      return finModel;
    }

    private void AddOotAnimationTrackToFin_(
        IAnimationTrack ootAnimationTrack,
        int axis,
        ISeparateEulerRadiansKeyframes<Keyframe<float>> rotations) {
      for (var f = 0; f < ootAnimationTrack.Frames.Count; ++f) {
        rotations
            .Axes[axis]
            .SetKeyframe(f,
                         (float) ((ootAnimationTrack.Frames[f] * 360.0) /
                                  0xFFFF));
      }
    }

    private static Quaternion ConvertRadiansToQuaternionOot_(
        float xRadians,
        float yRadians,
        float zRadians) {
      var r2d = MathF.PI / 180;
      var xDegrees = xRadians * r2d;
      var yDegrees = yRadians * r2d;
      var zDegrees = zRadians * r2d;

      var qz = Quaternion.CreateFromYawPitchRoll(0, 0, zDegrees);
      var qy = Quaternion.CreateFromYawPitchRoll(yDegrees, 0, 0);
      var qx = Quaternion.CreateFromYawPitchRoll(0, xDegrees, 0);

      return Quaternion.Normalize(qz * qy * qx);
    }
  }
}