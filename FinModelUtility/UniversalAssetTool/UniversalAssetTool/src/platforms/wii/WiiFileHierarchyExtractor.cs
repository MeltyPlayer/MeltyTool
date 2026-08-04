using fin.common;
using fin.io;

using uni.games;
using uni.platforms.wii.tools;

namespace uni.platforms.wii;

public sealed class WiiFileHierarchyExtractor {
  private readonly Wit wit_ = new();

  public bool TryToExtractFromGame(
      string gameName,
      out IFileHierarchy fileHierarchy) {
    if (!TryToFindRom(gameName, out var romFile)) {
      fileHierarchy = null;
      return false;
    }

    fileHierarchy = this.ExtractFromRom(romFile);
    return true;
  }

  public static bool HasRomOrExtractedDirectory(string gameName)
    => TryToFindRom(gameName, out _) ||
       ExtractorUtil.HasBeenExtracted(gameName);

  public static bool TryToFindRom(string gameName, out ISystemFile romFile)
    => DirectoryConstants.ROMS_DIRECTORY
                         .TryToGetExistingFileWithFileType(
                             gameName,
                             out romFile,
                             ".ciso",
                             ".iso");


  public IFileHierarchy ExtractFromRom(ISystemFile romFile) {
    this.wit_.Run(romFile, out var fileHierarchy);
    return fileHierarchy;
  }
}