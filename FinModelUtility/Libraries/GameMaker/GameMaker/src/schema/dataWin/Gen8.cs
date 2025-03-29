using schema.binary.attributes;
using schema.binary;

namespace gm.schema.dataWin;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/puggsoy/GMS-Explorer/blob/master/GMS%20Explorer/Chunks/GEN8.cs#L10
/// </summary>
[BinarySchema]
public partial class Gen8 : IBinaryConvertible {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool Debug { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT24)]
  public uint Unk0 { get; set; }

  private uint filenameOffset_;

  [NullTerminatedString]
  [RAtPosition(nameof(filenameOffset_))]
  public string FileName { get; set; }

  [SequenceLengthSource(12)]
  public byte[] Unk1 { get; set; }

  public uint GameId { get; }

  private uint nameOffset_;

  [NullTerminatedString]
  [RAtPosition(nameof(filenameOffset_))]
  public string Name { get; set; }

  public uint MajorVersion { get; set; }
  public uint MinorVersion { get; set; }
  public uint ReleaseVersion { get; set; }
  public uint BuildVersion { get; set; }

  [SequenceLengthSource(40)]
  public byte[] Unk2 { get; set; }

  private uint displayNameOffset_;

  [NullTerminatedString]
  [RAtPosition(nameof(displayNameOffset_))]
  public string DisplayName { get; set; }

  [SequenceLengthSource(20)]
  public byte[] Unk3 { get; set; }

  public uint SteamAppId { get; set; }
}