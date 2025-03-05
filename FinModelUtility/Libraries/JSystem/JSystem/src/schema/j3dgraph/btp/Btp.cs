using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.btp;

/// <summary>
///   BTP files define texture-swap animations.
///
///   https://wiki.cloudmodding.com/tww/BTP
/// </summary>
[Endianness(Endianness.BigEndian)]
[BinarySchema]
public partial class Btp : IBinaryConvertible;