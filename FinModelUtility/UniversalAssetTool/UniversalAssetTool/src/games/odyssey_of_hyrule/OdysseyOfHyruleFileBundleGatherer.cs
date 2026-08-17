using fin.io;
using fin.io.bundles;
using fin.util.progress;

using vrml.api;


namespace uni.games.odyssey_of_hyrule;

public sealed class OdysseyOfHyruleFileBundleGatherer
    : BPrereqsFileBundleGatherer {
  public override string Name => "odyssey_of_hyrule";

  public override FileBundleGathererPlatform Platform
    => FileBundleGathererPlatform.DESKTOP;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var wrlFile in fileHierarchy.Root.GetFilesWithFileType(".wrl")) {
      organizer.Add(new VrmlModelFileBundle { WrlFile = wrlFile.Impl });
      organizer.Add(new VrmlSceneFileBundle { WrlFile = wrlFile.Impl });
    }
  }
}