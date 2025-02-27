using fin.io.bundles;
using fin.util.progress;

using ssm.api;

using uni.platforms.gcn;

namespace uni.games.kirby_air_ride;

public class KirbyAirRideFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "kirby_air_ride",
            out var fileHierarchy)) {
      return;
    }

    // TODO: Support dat files, appear to be similar to Custom Robo?

    foreach (var ssmFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".ssm")) {
      organizer.Add(new SsmAudioFileBundle {
          SsmFile = ssmFile,
      }.Annotate(ssmFile));
    }
  }
}