using DanDare.api;

using fin.io;
using fin.io.bundles;
using fin.util.progress;


namespace uni.games.odyssey_of_hyrule;

public sealed class DanDareFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "dan_dare";
  public override string Title => "Dan-Dare";

  public override FileBundleGathererPlatform Platform
    => FileBundleGathererPlatform.DESKTOP;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var a3dFile in fileHierarchy.Root.GetFilesWithFileType(".a3d")) {
      organizer.Add(new A3dModelFileBundle(a3dFile.Impl));
    }
  }
}