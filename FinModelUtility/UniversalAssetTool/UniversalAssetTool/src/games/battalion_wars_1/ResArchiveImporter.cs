using System.IO.Compression;

using fin.archives;
using fin.data.dictionaries;
using fin.io;
using fin.io.archive;
using fin.util.asserts;

using modl.schema.res;

using schema.binary;

namespace uni.games.battalion_wars_1;

public sealed record ResArchiveFileBundle(ISystemFile ResFile)
    : ISimpleCleanableArchiveFileBundle {
  public IReadOnlyTreeFile MainFile => this.ResFile;
  public void CleanUp() => this.ResFile.Delete();
}

public sealed class ResArchiveImporter
    : BSimpleArchiveImporter<ResArchiveFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      ResArchiveFileBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    var resFile = bundle.ResFile;
    GetResFileAttributes_(resFile, out var isResGz, out _);

    baseStream = readStream = resFile.OpenRead();
    if (isResGz) {
      baseStream = new GZipStream(baseStream, CompressionMode.Decompress);
      readStream = new MemoryStream();
      baseStream.CopyTo(readStream);
      readStream.Position = 0;
    }

    if (!MagicTextUtil.Verify(readStream, "RXET")) {
      Asserts.Fail("Expected RES file to start with TEXR magic text!");
    }

    var br = new SchemaBinaryReader(readStream, Endianness.LittleEndian);
    var bwArchive = br.ReadNew<BwArchive>();

    foreach (var (bwFileExtension, bwFiles) in bwArchive.Files.GetPairs()) {
      foreach (var bwFile in bwFiles) {
        var fileName = $"{bwFile.FileName}.{bwFileExtension.ToLower()}";
        builderRoot.AddFile(fileName, bwFile.Position, bwFile.Length);
      }
    }

    foreach (var texture in bwArchive.TexrSection.Textures) {
      builderRoot.AddFile($"{texture.Name}.texr",
                          texture.Position,
                          texture.Length);
    }
  }

  private static void GetResFileAttributes_(
      IReadOnlyTreeFile resFile,
      out bool isGz,
      out ReadOnlySpan<char> directoryFullName) {
    directoryFullName = resFile.FullPath;
    isGz = resFile.Name.EndsWith(".res.gz");
    directoryFullName
        = isGz ? directoryFullName[..^7] : directoryFullName[..^4];
  }

  public static ArchiveExtractionResult Extract(ISystemFile resFile) {
    GetResFileAttributes_(resFile, out _, out var directoryFullName);
    return new ResArchiveImporter().ExtractIntoAndMaybeCleanUp(
        new ResArchiveFileBundle(resFile),
        new FinDirectory(directoryFullName.ToString()));
  }
}