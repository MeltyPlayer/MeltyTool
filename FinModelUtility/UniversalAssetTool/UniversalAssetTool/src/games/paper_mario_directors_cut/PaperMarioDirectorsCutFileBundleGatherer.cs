using fin.io.bundles;
using fin.util.progress;

using pmdc.api;

using uni.platforms;

namespace uni.games.paper_mario_directors_cut;

public class PaperMarioDirectorsCutFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join("paper_mario_directors_cut", ExtractorUtil.PREREQS),
            out var pmdcDir)) {
      return;
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("paper_mario_directors_cut", pmdcDir);

    foreach (var modFile in fileHierarchy.Root.GetFilesWithFileType(
                 ".mod",
                 true)) {
      organizer.Add(new AnnotatedFileBundle<ModModelFileBundle>(
                        new ModModelFileBundle {
                            GameName = "paper_mario_directors_cut",
                            ModFile = modFile
                        },
                        modFile));
    }

    foreach (var omdFile in fileHierarchy.Root.GetFilesWithFileType(
                 ".omd",
                 true)) {
      organizer.Add(new AnnotatedFileBundle<OmdModelFileBundle>(
                        new OmdModelFileBundle {
                            GameName = "paper_mario_directors_cut",
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