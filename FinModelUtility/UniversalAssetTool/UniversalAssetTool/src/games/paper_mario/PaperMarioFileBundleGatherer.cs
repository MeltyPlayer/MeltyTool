using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using pm.api;

namespace uni.games.paper_mario;

public sealed class PaperMarioFileBundleGatherer : BN64FileBundleGatherer {
  public override string Name => "paper_mario";

  protected override void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir,
      ISystemDirectory prereqsDir)
    => new PaperMarioFileTableImporter().ExtractInto(romFile, extractedDir);

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress
          mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var rootDirectoryImpl = fileHierarchy.Root.Impl;
  }
}