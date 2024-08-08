using fin.io.bundles;
using fin.util.progress;

using uni.platforms;

using vrml.api;


namespace uni.games.vrwdw;

public class VrwdwFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public string Name => "vrwdw";

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
      if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
              Path.Join("vrwdw", ExtractorUtil.PREREQS),
              out var vrwdwDir)) {
        return;
      }

      var fileHierarchy = ExtractorUtil.GetFileHierarchy("vrwdw", vrwdwDir);

      foreach (var wrlFile in fileHierarchy.Root.GetFilesWithFileType(".wrl")) {
        organizer.Add(new VrmlModelFileBundle {
            WrlFile = wrlFile,
        }.Annotate(wrlFile));
      }
    }
}