using fin.common;
using fin.io.bundles;
using fin.util.progress;

using vrml.api;


namespace uni.games.odyssey_of_hyrule;

public class OdysseyOfHyruleFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public string Name => "odyssey_of_hyrule";

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
      if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
              Path.Join("odyssey_of_hyrule", ExtractorUtil.PREREQS),
              out var vrwdwDir)) {
        return;
      }

      var fileHierarchy = ExtractorUtil.GetFileHierarchy("odyssey_of_hyrule", vrwdwDir);

      foreach (var wrlFile in fileHierarchy.Root.GetFilesWithFileType(".wrl")) {
        organizer.Add(new VrmlModelFileBundle {
            WrlFile = wrlFile,
        }.Annotate(wrlFile));
        organizer.Add(new VrmlSceneFileBundle {
            WrlFile = wrlFile,
        }.Annotate(wrlFile));
      }
    }
}