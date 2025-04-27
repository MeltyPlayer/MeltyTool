using System.IO;

using fin.io;
using fin.io.archive;

namespace fin.archives;

public static class ArchiveExtensions {
  public static ArchiveExtractionResult TryToImportAndExtractLocally<TBundle>(
      this IArchiveImporter<TBundle> importer,
      TBundle bundle,
      bool cleanUp = false) where TBundle : IArchiveBundle {
    var directory = new FinDirectory(bundle.MainFile.FullNameWithoutExtension);
    if (directory is { Exists: true, IsEmpty: false }) {
      return ArchiveExtractionResult.ALREADY_EXISTS;
    }

    importer.ImportAndExtractRelativeTo(bundle, directory, cleanUp);

    return ArchiveExtractionResult.NEWLY_EXTRACTED;
  }

  public static void ImportAndExtractRelativeTo<TBundle>(
      this IArchiveImporter<TBundle> importer,
      TBundle bundle,
      ISystemDirectory directory,
      bool cleanUp = false) where TBundle : IArchiveBundle {
    using (var archive = importer.Import(bundle)) {
      archive.ExtractRelativeTo(directory);
    }

    var mainFile = bundle.MainFile;
    if (cleanUp && mainFile != null) {
      new FinFile(mainFile).Delete();
    }
  }

  public static void ExtractRelativeTo(
      this IArchive archive,
      ISystemDirectory directory) {
    foreach (var fileEntry in archive.FileEntries) {
      var dstFile
          = new FinFile(Path.Join(directory.FullPath, fileEntry.FullPath));

      var dstDir = dstFile.AssertGetParent();
      dstDir.Create();

      using var fw = dstFile.OpenWrite();
      using var entryStream = fileEntry.OpenRead();
      entryStream.CopyTo(fw);
    }
  }
}