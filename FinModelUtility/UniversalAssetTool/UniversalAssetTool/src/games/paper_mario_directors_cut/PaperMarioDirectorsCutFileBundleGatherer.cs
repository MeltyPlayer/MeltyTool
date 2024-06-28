using fin.io;
using fin.io.bundles;

using pmdc.api;

using uni.platforms;

namespace uni.games.paper_mario_directors_cut {
  public class PaperMarioDirectorsCutFileBundleGatherer
      : IAnnotatedFileBundleGatherer<IFileBundle> {
    public IEnumerable<IAnnotatedFileBundle<IFileBundle>>
        GatherFileBundles() {
      if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
              Path.Join("paper_mario_directors_cut", ExtractorUtil.PREREQS),
              out var pmdcDir)) {
        yield break;
      }

      var fileHierarchy
          = FileHierarchy.From("paper_mario_directors_cut", pmdcDir);

      foreach (var omdFile in fileHierarchy.Root.GetFilesWithFileType(
                   ".omd",
                   true)) {
        yield return new AnnotatedFileBundle<OmdModelFileBundle>(
            new OmdModelFileBundle { OmdFile = omdFile },
            omdFile);
      }

      foreach (var lvlFile in fileHierarchy.Root.GetFilesWithFileType(
                   ".lvl",
                   true)) {
        yield return new AnnotatedFileBundle<LvlSceneFileBundle>(
            new LvlSceneFileBundle {
                LvlFile = lvlFile,
                RootDirectory = fileHierarchy.Root
            },
            lvlFile);
      }
    }
  }
}