using DiscUtils;
using DiscUtils.Iso9660;

using fin.common;
using fin.data.queues;
using fin.io;
using fin.util.strings;

using uni.games;

namespace uni.platforms;

public static class DiscFileHierarchyExtractor {
  // TODO: Support .bin/.cue

  public static bool HasRomOrExtractedDirectory(string gameName)
    => DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFileWithFileType(
           gameName,
           out _,
           ".iso") ||
       ExtractorUtil.HasBeenExtracted(gameName);

  public static bool TryToExtractFromGame(
      string gameName,
      out IFileHierarchy fileHierarchy) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFileWithFileType(
            gameName,
            out var isoFile,
            ".iso")) {
      fileHierarchy = null;
      return false;
    }

    var extractedDir = ExtractorUtil.GetOrCreateExtractedDirectory(gameName);

    using var cdReader = new CDReader(isoFile.OpenRead(), true);

    var directoryQueue = new FinQueue<string>(cdReader.Root.FullName);
    while (directoryQueue.TryDequeue(out var currentDirPath)) {
      new FinDirectory(Path.Join(extractedDir.FullPath, currentDirPath)).Create();

      foreach (var rawLocalFilePath in cdReader.GetFiles(currentDirPath)) {
        var fileStream = cdReader.OpenFile(rawLocalFilePath, FileMode.Open);

        var localFilePath = rawLocalFilePath.SubstringUpTo(';');
        var dstFile = new FinFile(Path.Join(extractedDir.FullPath, localFilePath));
        using var dstStream = dstFile.OpenWrite();

        fileStream.CopyTo(dstStream);
      }

      directoryQueue.Enqueue(cdReader.GetDirectories(currentDirPath));
    }

    fileHierarchy = ExtractorUtil.GetFileHierarchy(gameName, extractedDir);

    return true;
  }
}