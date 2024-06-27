using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.sklm;

[BinarySchema]
public partial class PrimitiveSet : IBinaryConvertible {
  private readonly string magic_ = "prms";

  public uint chunkSize;

  // Actually an array but more than one is never used
  public readonly uint primitiveCount = 1;
    
  public SkinningMode skinningMode;
  private ushort boneTableCount;
  public uint boneTableOffset;
  public uint primitiveOffset;

  [RSequenceLengthSource(nameof(boneTableCount))]
  public short[] boneTable;

  [Align(4)] public readonly Primitive primitive = new();
}