using fin.io;
using fin.io.bundles;
using fin.util.progress;

using ssm.api;

namespace uni.games.kirby_air_ride;

public class KirbyAirRideFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "kirby_air_ride";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    // TODO: Support dat files, appear to be similar to Custom Robo?

    foreach (var ssmFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".ssm")) {
      organizer.Add(new SsmAudioFileBundle {
          SsmFile = ssmFile,
      }.Annotate(ssmFile));
    }
  }
}