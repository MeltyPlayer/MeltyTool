using dat.api;

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

    foreach (var datFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".dat")) {
      organizer.Add(new DatModelFileBundle {
          GameName = "kirby_air_ride",
          DatFile = datFile
      }.Annotate(datFile));
    }

    foreach (var ssmFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".ssm")) {
      organizer.Add(new SsmAudioFileBundle {
          GameName = "kirby_air_ride",
          SsmFile = ssmFile,
      }.Annotate(ssmFile));
    }
  }
}