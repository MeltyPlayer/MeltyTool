using fin.io;
using fin.io.bundles;
using fin.util.progress;

using ssm.api;

namespace uni.games.custom_robo;

public class CustomRoboFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "custom_robo";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var ssmFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".ssm")) {
      organizer.Add(new SsmAudioFileBundle {
          SsmFile = ssmFile,
      }.Annotate(ssmFile));
    }
  }
}