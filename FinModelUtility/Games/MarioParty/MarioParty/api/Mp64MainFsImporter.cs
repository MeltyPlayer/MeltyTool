using fin.archives;
using fin.io;

using LinkDotNet.StringBuilder;

using mp64.schema.mainfs;

using schema.binary;

namespace mp64.api;

public sealed record Mp64RomFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/fs/mainfs.ts
/// </summary>
public sealed class Mp64MainFsImporter
    : BSimpleArchiveImporter<Mp64RomFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      Mp64RomFileBundle bundle,
      ISet<IReadOnlyStandaloneFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.MainFile.OpenRead();

    var romBr = new SchemaBinaryReader(readStream, Endianness.BigEndian);

    romBr.Position = 0x3b;
    var romCode = romBr.ReadString(4);
    var game = Game.FromRomCode(romCode);

    var mainFsOffsets = game.GetMainFsOffsets();
    var fsOffset = GetRomOffset_(mainFsOffsets);

    readStream.Position = fsOffset;

    var mainFs = romBr.ReadNew<MainFs>();

    var dirPtrs = mainFs.Directories;
    for (var d = 0; d < dirPtrs.Length; ++d) {
      var dir = dirPtrs[d].Value;

      var builderDir = builderRoot.AddSubdir($"{d}");

      var filePtrs = dir.Files;
      for (var f = 0; f < filePtrs.Length; ++f) {
        var file = filePtrs[f].Value;

        var data = file.DecompressedBytes;

        using var nameSb = new ValueStringBuilder();
        nameSb.Append(f);
        if (MagicTextUtil.Verify(data, "FORM")) {
          nameSb.Append(".form");
        } else if (MagicTextUtil.Verify(data, "MTNX")) {
          nameSb.Append(".mtnx");
        } else {
          nameSb.Append(".bin");
        }

        builderDir.AddFile(nameSb.ToString(), data);
      }
    }
  }

  private static uint GetRomOffset_((uint, uint)[] mainFsOffsets) {
    var (upper, lower) = mainFsOffsets[0];
    var offset = (upper << 16) | lower;
    if ((lower & 0x8000) != 0) {
      // Account for signed addition workaround.
      offset -= 0x00010000;
    }

    return offset;
  }
}