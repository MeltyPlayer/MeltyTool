using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games;

public abstract class BN64FileBundleGatherer
    : INamedFileBundleGatherer {
  public abstract string Name { get; }

  protected abstract void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir);

  protected abstract void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants
         .ROMS_DIRECTORY
         .TryToGetExistingFileWithFileType(
             this.Name,
             out var romFile,
             ".z64")) {
      return;
    }

    var extractedDir = ExtractorUtil.GetOrCreateExtractedDirectory(
        this.Name);
    if (extractedDir.IsEmpty) {
      this.ExtractFilesFromRom(romFile, extractedDir);
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy(this.Name, extractedDir);

    this.GatherFileBundlesFromHierarchy(
        organizer,
        mutablePercentageProgress,
        fileHierarchy);
  }
}