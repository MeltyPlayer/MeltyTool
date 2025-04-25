using fin.schema.data;

using schema.binary;
using schema.binary.attributes;

namespace nitro.schema.narc;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/nickworonekin/narchive/blob/master/src/Narchive/Formats/NarcArchive.cs
/// </summary>
[BinarySchema]
public partial class Fatb : IBinaryConvertible {
  public AutoStringMagicUInt32SizedSection<FatbData> Data { get; }
    = new("FATB");
}

[BinarySchema]
public partial class FatbData : IBinaryConvertible {
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public FatbEntry[] Entries { get; set; }
}

[BinarySchema]
public partial class FatbEntry : IBinaryConvertible {
  public uint Offset { get; set; }
  public uint Length { get; set; }
}