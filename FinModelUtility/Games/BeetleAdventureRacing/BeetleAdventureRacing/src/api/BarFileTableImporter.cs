using bar.schema;

using fin.archives;
using fin.io;
using fin.schema.data;
using fin.util.streams;

using schema.binary;

namespace bar.api;

public sealed record BarRomFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/Filesystem.ts
/// </summary>
public sealed class BarFileTableImporter
    : BSimpleArchiveImporter<BarRomFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      BarRomFileBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.MainFile.OpenRead();

    var crc32 = readStream.CalculateCrc32();

    var fileTableOffset = crc32 switch {
        0xf4a97c73 => 0x237d0,
        _ => throw new NotImplementedException(
            $"Unsupported Beetle Adventure Racing ROM crc32: {crc32:0X}")
    };

    readStream.Position = fileTableOffset;

    var romBr = new SchemaBinaryReader(readStream, Endianness.BigEndian);

    var section = new AutoStringMagicUInt32SizedSection<Uvft>("FORM");
    section.Read(romBr);

    romBr.Align(0x10);
    var fileDataOffset = romBr.Position;

    var uvft = section.Data;
    foreach (var uvftFileType in uvft.FileTypes) {
      for (var i = 0; i < uvftFileType.Offsets.Length; ++i) {
        var entryOffset = uvftFileType.Offsets[i];

        uint currentFilePosition;
        uint fileLength;
        if (entryOffset == -1) {
          currentFilePosition = 0;
          fileLength = 0;
        } else {
          currentFilePosition = (uint) (fileDataOffset + entryOffset);
          fileLength = romBr.SubreadAt(
              currentFilePosition,
              () => {
                var str = romBr.ReadString(4);
                if (str == "FORM") {
                  return 4 + 4 + romBr.ReadUInt32();
                }

                return (uint) 0;
              });
        }

        var type = uvftFileType.Type.ToLower();
        builderRoot.AddFile(
            $"{type}/{i}.{type}",
            currentFilePosition,
            fileLength);
      }
    }
  }
}