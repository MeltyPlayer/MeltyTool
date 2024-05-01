using System.IO;

using fin.io;
using fin.io.bundles;

using pmdc.api;

using uni.platforms;

namespace uni.games.paper_mario_directors_cut {
  public class PaperMarioDirectorsCutFileBundleGatherer
      : IAnnotatedFileBundleGatherer<OmdModelFileBundle> {
    public IEnumerable<IAnnotatedFileBundle<OmdModelFileBundle>>
        GatherFileBundles() {
      if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
              Path.Join("paper_mario_directors_cut", ExtractorUtil.PREREQS),
              out var pmdcDir)) {
        yield break;
      }

      var fileHierarchy
          = FileHierarchy.From("paper_mario_directors_cut", pmdcDir);

      var omdFiles = fileHierarchy.Root.GetFilesWithFileType(".omd", true);
      foreach (var omdFile in omdFiles) {
        yield return new AnnotatedFileBundle<OmdModelFileBundle>(
            new OmdModelFileBundle { OmdFile = omdFile },
            omdFile);
      }
    }
  }
}