using fin.archives;
using fin.config;
using fin.io;

using nitro.schema.narc;

using schema.binary;

namespace nitro.api;

public record NarcArchiveBundle(IReadOnlyTreeFile NarcFile)
    : IUncompressedArchiveBundle {
  public IReadOnlyTreeFile ArchiveFile => this.NarcFile;
}

public class NarcArchiveImporter
    : BUncompressedArchiveImporter<NarcArchiveBundle> {
  protected override IEnumerable<UncompressedArchiveSubFile> EnumerateSubFiles(
      IBinaryReader archiveBr,
      NarcArchiveBundle bundle)
    => archiveBr.ReadNew<Narc>().FileEntries;

  public static void ImportAndExtractAll(
      IFileHierarchy fileHierarchy,
      bool cleanUp = false) {
    var narcFiles = fileHierarchy.Root.GetFilesWithFileType(".narc", true)
                                 .ToArray();
    if (narcFiles.Length == 0) {
      return;
    }

    var dataDir = fileHierarchy.Root.Impl.AssertGetExistingSubdir("data");
    var narcImporter = new NarcArchiveImporter();
    foreach (var narcFile in narcFiles) {
      narcImporter.ImportAndExtractRelativeTo(
          new NarcArchiveBundle(narcFile),
          dataDir,
          FinConfig.CleanUpArchives);
    }

    fileHierarchy.RefreshRootAndUpdateCache();
  }
}