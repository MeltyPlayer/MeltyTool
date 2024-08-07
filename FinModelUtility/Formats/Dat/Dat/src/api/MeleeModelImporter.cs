using dat.schema;
using dat.schema.animation;
using dat.schema.melee;

using fin.data.lazy;
using fin.io;
using fin.model;
using fin.model.io.importers;
using fin.util.strings;

using schema.binary;

namespace dat.api;

public class MeleeModelImporter : IModelImporter<MeleeModelFileBundle> {
  public IModel Import(MeleeModelFileBundle modelFileBundle) {
    var datModelFileBundle = new DatModelFileBundle {
        GameName = modelFileBundle.GameName,
        DatFile = modelFileBundle.PrimaryDatFile,
    };

    var finModel = new DatModelImporter().Import(
        datModelFileBundle,
        out var files,
        out var primaryDat,
        out var finBoneByJObj,
        out var sortedJObjsAndDObjs,
        out var finMeshByDObj);

    var animationDatFile = modelFileBundle.AnimationDatFile;
    if (animationDatFile != null) {
      files.Add(animationDatFile);
    }

    var fighterDatFile = modelFileBundle.FighterDatFile;
    if (fighterDatFile != null) {
      files.Add(fighterDatFile);
    }

    var animationDat = animationDatFile?.ReadNew<Dat>(Endianness.BigEndian);
    var fighterDatSubfile = fighterDatFile?
                            .ReadNew<Dat>(Endianness.BigEndian)
                            .Subfiles
                            .Single();

    // Adds animations
    if (animationDat != null) {
      var lazyFinAnimations = new LazyList<IModelAnimation>(i => {
        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = $"Animation {i}";

        finAnimation.FrameRate = 60;
        finAnimation.UseLoopingInterpolation = false;

        return finAnimation;
      });

      var i = 0;

      var jObjs = primaryDat.JObjs.ToArray();
      foreach (var animationDatSubfile in animationDat.Subfiles) {
        foreach (var (figaTree, figaTreeName) in animationDatSubfile
                     .GetRootNodesWithNamesOfType<FigaTree>()) {
          var finAnimation = lazyFinAnimations[i++];
          finAnimation.Name = figaTreeName.SubstringAfter("Share_ACTION_")
                                          .SubstringUpTo("_figatree");
          finAnimation.FrameCount = (int) figaTree.FrameCount;

          foreach (var (jObj, trackNode) in jObjs.Zip(figaTree.TrackNodes)) {
            var finBone = finBoneByJObj[jObj];
            var boneTracks = finAnimation.AddBoneTracks(finBone);

            DatBoneTracksHelper.AddDatKeyframesToBoneTracks(trackNode,
              boneTracks);
          }
        }
      }
    }

    // Hides low-poly meshes based on data from fighter file
    List<HashSet<byte>>? lowPolyDObjsByJObj = null;
    if (fighterDatSubfile != null) {
      var fighterData =
          fighterDatSubfile.GetRootNodesOfType<MeleeFighterData>()
                           .Single();

      var lowPoly = fighterData.ModelLookupTables
                               .CostumeVisibilityLookupTable
                               ?.LowPoly;
      if (lowPoly != null) {
        lowPolyDObjsByJObj = [];
        foreach (var lookupEntry in lowPoly.LookupEntries) {
          var set = new HashSet<byte>();
          lowPolyDObjsByJObj.Add(set);

          foreach (var byteEntry in lookupEntry.ByteEntries) {
            set.Add(byteEntry);
          }
        }
      }
    }

    foreach (var (_, jObjIndex, dObj, dObjIndex) in sortedJObjsAndDObjs) {
      if (lowPolyDObjsByJObj != null && jObjIndex < lowPolyDObjsByJObj.Count) {
        var lowPolyDObjsSet = lowPolyDObjsByJObj[jObjIndex];
        if (lowPolyDObjsSet.Contains(dObjIndex)) {
          finMeshByDObj[dObj].DefaultDisplayState = MeshDisplayState.HIDDEN;
        }
      }
    }

    return finModel;
  }
}