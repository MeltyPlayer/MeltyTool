using fin.io;
using fin.io.archive;

using uni.platforms.threeDs.tools;
using uni.platforms.threeDs.tools.ctrtool;
using uni.platforms.threeDs.tools.gar;

namespace uni.platforms.threeDs;

public class ThreeDsFileHierarchyExtractor {
  public bool TryToExtractFromGame(string gameName,
                                   out IFileHierarchy fileHierarchy,
                                   IArchiveExtractor.ArchiveFileProcessor? archiveFileNameProcessor = null) {
    if (!TryToFindRom_(gameName, out var romFile)) {
      fileHierarchy = default;
      return false;
    }

    fileHierarchy = this.ExtractFromRom_(romFile, archiveFileNameProcessor);
    return true;
  }

  private static bool TryToFindRom_(string gameName, out IReadOnlySystemFile romFile)
    => DirectoryConstants.ROMS_DIRECTORY
                         .TryToGetExistingFileWithFileType(
                             gameName,
                             out romFile,
                             ".cci",
                             ".3ds",
                             ".cia");

  private IFileHierarchy ExtractFromRom_(IReadOnlySystemFile romFile,
                                         IArchiveExtractor.ArchiveFileProcessor? archiveFileNameProcessor = null) {
    IFileHierarchy fileHierarchy;
    switch (romFile.FileType) {
      case ".cia": {
        new Ctrtool.CiaExtractor().Run(romFile, out fileHierarchy);
        break;
      }
      case ".3ds":
      case ".cci": {
        new Ctrtool.CciExtractor().Run(romFile, out fileHierarchy);
        break;
      }
      default: throw new NotSupportedException();
    }

    var rootDir = fileHierarchy.Root.Impl;

    var archiveExtractor = new SubArchiveExtractor();

    var didDecompress = false;
    foreach (var directory in fileHierarchy) {
      var didChange = false;
      foreach (var zarFile in directory.FilesWithExtension(".zar")) {
        didChange |=
            archiveExtractor.TryToExtractIntoNewDirectory<ZarReader>(
                zarFile,
                rootDir,
                archiveFileNameProcessor) ==
            ArchiveExtractionResult.NEWLY_EXTRACTED;
      }

      foreach (var garFile in directory.FilesWithExtension(".gar")) {
        didChange |=
            archiveExtractor.TryToExtractIntoNewDirectory<GarReader>(
                garFile,
                rootDir,
                archiveFileNameProcessor) ==
            ArchiveExtractionResult.NEWLY_EXTRACTED;
      }

      foreach (var garFile in directory.GetExistingFiles().Where(
                   file => file.Name.EndsWith(".gar.lzs"))) {
        didChange |=
            archiveExtractor.TryToExtractIntoNewDirectory<GarReader>(
                garFile,
                rootDir,
                archiveFileNameProcessor) ==
            ArchiveExtractionResult.NEWLY_EXTRACTED;
      }

      if (didChange) {
        directory.Refresh();
      }
    }

    if (didDecompress) {
      fileHierarchy.Root.Refresh(true);
    }

    return fileHierarchy;
  }
}