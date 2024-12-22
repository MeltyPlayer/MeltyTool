using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bmd;

/// <summary>
///   Shamelessly stolen from:
///   https://kuribo64.net/get.php?id=KBNyhM0kmNiuUBb3
/// </summary>
[BinarySchema]
public partial class Palette : IBinaryConvertible {
  private uint nameOffset_;

  [NullTerminatedString]
  [RAtPosition(nameof(nameOffset_))]
  public string Name { get; set; }

  private uint dataOffset_;
  private uint dataLength_;

  [RAtPosition(nameof(dataOffset_))]
  [RSequenceLengthSource(nameof(dataLength_))]
  public byte[] Data { get; set; }

  [Unknown]
  public uint Unknown { get; set; }
}