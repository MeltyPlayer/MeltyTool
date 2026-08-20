using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using hm64.api;

namespace uni.games.harvest_moon_64;

public sealed class HarvestMoon64FileBundleGatherer : BN64FileBundleGatherer {
  public override string Name => "harvest_moon_64";

  protected override void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir,
      ISystemDirectory prereqsDir)
    => new Hm64FileTableImporter(
        prereqsDir.AssertGetExistingFile("map_addresses.csv")).ExtractInto(
        romFile,
        extractedDir);

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress
          mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}