using System.IO.Hashing;

using CommunityToolkit.Diagnostics;

using fin.io;
using fin.math;

using schema.binary;
using schema.binary.attributes;

namespace MarioPicross;

public class PuzzleDefinitionReader {
  public PuzzleDefinition[]? Read(IReadOnlyGenericFile romFile) {
    var romData = romFile.ReadAllBytes();
    var romCrc32 = Crc32.HashToUInt32(romData);
    if (!Constants.ENTRY_OFFSET_BY_FILE_CRC_32.TryGetValue(
            romCrc32,
            out var entryOffset)) {
      return null;
    }

    using var br = new SchemaBinaryReader(romData);
    br.Position = entryOffset;
    return br.ReadNews<PuzzleDefinition>(255);
  }
}

/// <summary>
///   Shamelessly stolen from:
///   https://www.zophar.net/fileuploads/3/21546xutra/picrossleveldata.txt
///   https://github.com/sopoforic/cgrr-mariospicross/blob/master/mariospicross.py
/// </summary>
[BinarySchema]
public partial class PuzzleDefinition : IBinaryConvertible {
  [SequenceLengthSource(15)]
  public ushort[] Rows { get; set; }

  public byte Width { get; set; }
  public byte Height { get; set; }

  public bool this[byte x, byte y] {
    get {
      Guard.IsLessThan(x, this.Width);
      Guard.IsLessThan(y, this.Height);
      return this.Rows[y].GetBit(x);
    }
    set {
      Guard.IsLessThan(x, this.Width);
      Guard.IsLessThan(y, this.Height);
      this.Rows[y]
          = value ? this.Rows[y].SetBitTo1(x) : this.Rows[y].SetBitTo0(x);
    }
  }
}