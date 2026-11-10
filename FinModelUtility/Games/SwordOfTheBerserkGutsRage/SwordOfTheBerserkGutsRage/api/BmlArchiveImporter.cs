using berserkDc.schema.bml;

using fin.archives;
using fin.io;

using schema.binary;

namespace berserkDc.api;

public sealed record BmlArchiveFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleCleanableArchiveFileBundle {
  public void CleanUp() {
    // TODO
  }
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/theanine3D/pso_ultimate_importer/blob/main/__init__.py#L3153
/// </summary>
public sealed class BmlArchiveImporter
    : BSimpleArchiveImporter<BmlArchiveFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      BmlArchiveFileBundle bundle,
      ISet<IReadOnlyStandaloneFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.MainFile.OpenRead();

    var br = new SchemaBinaryReader(readStream);

    br.PushContainerEndianness(Endianness.BigEndian);
    var bigEndianValue = br.ReadUInt32();
    br.PopEndianness();

    br.Position = 0;
    br.PushContainerEndianness(Endianness.LittleEndian);
    var littleEndianValue = br.ReadUInt32();
    br.PopEndianness();

    br.Position = 0;
    br.PushContainerEndianness(
        bigEndianValue < littleEndianValue
            ? Endianness.BigEndian
            : Endianness.LittleEndian);

    var bml = br.ReadNew<Bml>();
    br.Position = (br.Position + 0x7FF) & 0xFFFFF800;

    foreach (var entry in bml.Entries) {
      while (!br.Eof) {
        if (br.ReadByte() != 0) {
          --br.Position;
          break;
        }
      }

      if (br.Eof) {
        break;
      }

      builderRoot.AddFile(
          entry.Name,
          br.Position,
          entry.CompressedSize,
          src => DecompressPrs_(src, entry));
    }
  }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/theanine3D/pso_ultimate_importer/blob/main/__init__.py#L3108
  /// </summary>
  private static Stream DecompressPrs_(Stream src, BmlEntry entry) {
    List<int> iofs = [0];
    var bit_count = 0;
    byte cmd_byte = 0;

    bool _eof() => src.Position >= src.Length;

    byte _byte() => (byte) src.ReadByte();

    byte _bit() {
      if (bit_count == 0) {
        cmd_byte = _byte();
        bit_count = 8;
      }

      var b = cmd_byte & 1;
      cmd_byte >>= 1;
      --bit_count;
      return (byte) b;
    }

    var dst = new List<byte>((int) entry.DecompressedSize);
    while (!_eof()) {
      if (_bit() == 1) {
        dst.Add(_byte());
      } else {
        int start;
        int amount;
        if (_bit() == 1) {
          var a = _byte();
          var b = _byte();

          var offset = ((b << 8) | a) >> 3;
          amount = a & 7;

          if (!_eof()) {
            if (amount == 0) {
              amount = _byte() + 1;
            } else {
              amount += 2;
            }
          }

          start = dst.Count - 0x2000 + offset;
        } else {
          amount = (_bit() << 1) | _bit();

          var offset = _byte();
          amount += 2;

          start = dst.Count - 0x100 + offset;
        }

        for (var i = 0; i < amount; ++i) {
          if (0 <= start && start < dst.Count) {
            dst.Add(dst[start++]);
          } else {
            dst.Add(0);
          }
        }
      }
    }

    return new MemoryStream(dst.ToArray());
  }
}