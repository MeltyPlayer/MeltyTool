using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using mp64.api;

namespace uni.games.mario_party_2;

public sealed class MarioParty2FileBundleGatherer
    : BN64FileBundleGatherer {
  public override string Name => "mario_party_2";
  public override string Title => "Mario Party 2";

  protected override void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir,
      ISystemDirectory prereqsDir)
    => new Mp64MainFsImporter()
        .ExtractInto(new Mp64RomFileBundle(romFile), extractedDir);

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress
          mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
  }
}