using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games;

public abstract class BN64FileBundleGatherer : INamedFileBundleGatherer {
  public abstract string Name { get; }
  public FileBundleGathererPlatform Platform => FileBundleGathererPlatform.N64;

  public bool IsAvailable
    => DirectoryConstants
       .ROMS_DIRECTORY
       .TryToGetExistingFileWithFileType(this.Name, out _, ".z64") ||
       ExtractorUtil.HasBeenExtracted(this.Name);

  protected abstract void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir,
      ISystemDirectory prereqsDir);

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

    ExtractorUtil.GetOrCreateRomDirectoriesWithPrereqs(
        this.Name,
        out var prereqsDir,
        out var extractedDir);
    if (extractedDir.IsEmpty) {
      this.ExtractFilesFromRom(romFile, extractedDir, prereqsDir);
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy(this.Name, extractedDir);

    this.GatherFileBundlesFromHierarchy(
        organizer,
        mutablePercentageProgress,
        fileHierarchy);
  }
}