using dk64.api;

using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.donkey_kong_64;

public sealed class DonkeyKong64FileBundleGatherer : BN64FileBundleGatherer {
  public override string Name => "donkey_kong_64";

  protected override void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir,
      ISystemDirectory prereqsDir)
    => new Dk64FileTableImporter().ExtractInto(
        new Dk64RomFileBundle(romFile),
        extractedDir);

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}