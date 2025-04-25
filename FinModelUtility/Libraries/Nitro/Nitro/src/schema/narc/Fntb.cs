using fin.schema.data;

using schema.binary;

namespace nitro.schema.narc;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/nickworonekin/narchive/blob/master/src/Narchive/Formats/NarcArchive.cs
/// </summary>
[BinarySchema]
public partial class Fntb : IBinaryConvertible {
  public AutoStringMagicUInt32SizedSection<FntbData> Data { get; }
    = new("FNTB");
}

// TODO: Handle when FNTB has no names
[BinarySchema]
public partial class FntbData : IBinaryConvertible {
  public uint RootNameEntryOffset { get; set; }
  public uint RootFirstFileIndex { get; set; }
  public uint RootFirstFileInde { get; set; }
}