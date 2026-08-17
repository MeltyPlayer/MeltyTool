using facade.api;

using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.facade;

public sealed class FacadeFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "facade";

  public override FileBundleGathererPlatform Platform
    => FileBundleGathererPlatform.DESKTOP;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var root = fileHierarchy.Root;

    var roomFile = root.AssertGetExistingFile("room.placeholder");
    organizer.Add(new FacadeRoomModelFileBundle(roomFile.Impl, root.Impl));
  }
}