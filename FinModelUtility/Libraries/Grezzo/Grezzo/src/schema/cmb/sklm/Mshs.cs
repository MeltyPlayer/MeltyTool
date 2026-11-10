using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.sklm;

[BinarySchema]
public sealed partial class Mshs : IBinaryConvertible, IChildOf<Sklm> {
  public Sklm Parent { get; set; }

  public readonly string magic = "mshs";
  public uint chunkSize;

  [WLengthOfSequence(nameof(Meshes))]
  private uint meshCount_;

  // The remainder are translucent meshes and always packed at the end
  public ushort opaqueMeshCount; 
  public ushort idCount;
    
  // Note: Mesh order = draw order
  [RSequenceLengthSource(nameof(meshCount_))]
  public Mesh[] Meshes { get; set; }
}