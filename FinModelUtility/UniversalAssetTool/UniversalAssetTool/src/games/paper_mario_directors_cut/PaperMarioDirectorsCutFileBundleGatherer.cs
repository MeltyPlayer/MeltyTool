using fin.io;
using fin.io.bundles;
using fin.util.progress;

using gm.api;

using pmdc.api;

namespace uni.games.paper_mario_directors_cut;

public class PaperMarioDirectorsCutFileBundleGatherer
    : BPrereqsFileBundleGatherer {
  public override string Name => "paper_mario_directors_cut";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var modFile in fileHierarchy.Root.GetFilesWithFileType(
                 ".mod",
                 true)) {
      organizer.Add(new AnnotatedFileBundle<D3dModelFileBundle>(
                        new D3dModelFileBundle {
                            ModFile = modFile
                        },
                        modFile));
    }

    foreach (var omdFile in fileHierarchy.Root.GetFilesWithFileType(
                 ".omd",
                 true)) {
      organizer.Add(new AnnotatedFileBundle<OmdModelFileBundle>(
                        new OmdModelFileBundle {
                            OmdFile = omdFile
                        },
                        omdFile));
    }

    foreach (var lvlFile in fileHierarchy.Root.GetFilesWithFileType(
                 ".lvl",
                 true)) {
      organizer.Add(new AnnotatedFileBundle<LvlSceneFileBundle>(
                        new LvlSceneFileBundle {
                            LvlFile = lvlFile,
                            RootDirectory = fileHierarchy.Root
                        },
                        lvlFile));
    }
  }
}