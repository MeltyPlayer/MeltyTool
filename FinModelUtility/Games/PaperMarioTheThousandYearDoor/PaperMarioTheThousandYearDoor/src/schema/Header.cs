using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema {
  [BinarySchema]
  public partial class Header : IBinaryDeserializable {
    public uint AnimationOffset { get; set; }

    [StringLengthSource(64)]
    public string ModelFileName { get; set; }

    [StringLengthSource(64)]
    public string TextureFileName { get; set; }

    [StringLengthSource(64)]
    public string CreationDate { get; set; }

    [SequenceLengthSource(3)]
    public uint[] Unk1 { get; set; }

    public Vector3f BoundingBoxMin { get; set; }
    public Vector3f BoundingBoxMax { get; set; }

    [SequenceLengthSource(25)]
    public int[] BlockTypeCounts { get; set; }

    [SequenceLengthSource(25)]
    public uint[] BlockTypeOffsets { get; set; }


    public int GetCount(BlockType blockType)
      => this.BlockTypeCounts[(int) blockType];

    public uint GetOffset(BlockType blockType)
      => this.BlockTypeOffsets[(int) blockType];
  }
}