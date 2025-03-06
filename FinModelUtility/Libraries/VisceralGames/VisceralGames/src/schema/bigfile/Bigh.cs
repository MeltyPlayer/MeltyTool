using schema.binary;
using schema.binary.attributes;

namespace visceral.schema.bigfile;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/gibbed/Gibbed.Visceral/blob/master/projects/Gibbed.Visceral.FileFormats/BigFile.cs
/// </summary>
[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Bigh : IBinaryDeserializable {
  private readonly string magic_ = "BIGH";

  [WSizeOfStreamInBytes]
  public uint TotalFileSize { get; set; }

  private uint entryCount_;
  public uint HeaderSize { get; set; }

  [RSequenceLengthSource(nameof(entryCount_))]
  public BighEntryInfo[] EntryInfos { get; set; }
}

[BinarySchema]
public partial class BighEntryInfo : IBinaryDeserializable {
  public uint Offset { get; set; }
  public uint Size { get; set; }
  public uint NameHash { get; set; }
}