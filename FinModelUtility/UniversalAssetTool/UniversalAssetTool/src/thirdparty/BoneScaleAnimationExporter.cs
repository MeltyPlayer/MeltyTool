using System.Numerics;

using fin.data.indexable;
using fin.io;
using fin.model;

namespace uni.thirdparty;

public class BoneScaleAnimationExporter {
  public void Export(IGenericFile luaFile, IReadOnlyModel model) {
      var animations = model.AnimationManager.Animations;
      if (animations.Count == 0) {
        return;
      }

      using var fw = luaFile.OpenWriteAsText();

      fw.WriteLine("local defScale = Vector(1,1,1)");
      fw.WriteLine(
          "ScalerKeysTable = { // Animation name => Bone names => frame scale keys. The keys are in frame order so 1,2,3,4 etc");

      foreach (var animation in animations) {
        var definedBones = new Dictionary<IReadOnlyBone, IReadOnlyBoneTracks>();
        foreach (var bone in model.Skeleton) {
          if (!animation.BoneTracks.TryGetValue(bone, out var boneTracks)) {
            continue;
          }

          var scales = boneTracks.Scales;
          if (scales is not { HasAnyData: true }) {
            continue;
          }

          for (var f = 0; f < animation.FrameCount; ++f) {
            scales.TryGetAtFrame(f, out var scale);

            if (!IsScaleOne(scale)) {
              definedBones[bone] = boneTracks;
              break;
            }
          }
        }

        if (definedBones.Count == 0) {
          continue;
        }

        fw.WriteLine($"  [\"{animation.Name}\"] = {{");

        foreach (var (bone, boneTracks) in definedBones) {
          var scales = boneTracks.Scales;

          fw.WriteLine($"    [\"{bone.Name}\"] = {{");

          for (var f = 0; f < animation.FrameCount; ++f) {
            scales.TryGetAtFrame(f, out var scale);

            if (IsScaleOne(scale)) {
              fw.WriteLine("      defScale,");
            } else {
              fw.WriteLine(
                  $"      Vector({scale.X:0.##}, {scale.Y:0.##}, {scale.Z:0.##}),");
            }
          }

          fw.WriteLine("    },");
        }

        fw.WriteLine("  },");
      }

      fw.WriteLine("}");
    }

  private static bool IsScaleOne(Vector3 scale)
    => Math.Abs(scale.X - 1) < .001
       && Math.Abs(scale.Y - 1) < .001
       && Math.Abs(scale.Z - 1) < .001;
}