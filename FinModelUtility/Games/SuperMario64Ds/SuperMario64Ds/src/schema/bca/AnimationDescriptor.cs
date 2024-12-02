using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bca;

/// <summary>
///   Shamelessly stolen from:
///   https://kuribo64.net/get.php?id=KNIJfjt4ZG2tcgDM
///   https://github.com/Arisotura/SM64DSe/blob/master/SM64DSFormats/BCA.cs
/// </summary>
[BinarySchema]
public partial class AnimationDescriptor : IBinaryDeserializable {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool InterpolateOddFrames { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool IncrementOffsetsEachFrame { get; set; }

  public ushort FirstIndex { get; set; }
}