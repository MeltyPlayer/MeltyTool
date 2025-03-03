using fin.schema;

using gx;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class IndirectTexture : IIndirectTexture, IBinaryConvertible {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool HasLookup { get; set; }

  [Unknown]
  public byte TevStageNum { get; set; }

  private readonly ushort padding_ = ushort.MaxValue;

  [SequenceLengthSource(4)]
  public IndTexOrder[] IndTexOrder { get; set; }

  [SequenceLengthSource(3)]
  public IndTexMatrix[] IndTexMatrix { get; set; }

  [SequenceLengthSource(4)]
  public IndTexCoordScale[] IndTexCoordScale { get; set; }

  [SequenceLengthSource(16)]
  public TevIndirect[] TevIndirect { get; set; }

  [Skip]
  public GxTexCoord TexCoord => this.IndTexOrder[0].TexCoord;

  [Skip]
  public GxTexMap TexMap => this.IndTexOrder[0].TexMap;
}

[BinarySchema]
public partial class IndTexOrder : IBinaryConvertible {
  public GxTexCoord TexCoord { get; set; }
  public GxTexMap TexMap { get; set; }
  private ushort Unknown { get; set; }
}

[BinarySchema]
public partial class IndTexMatrix : IBinaryConvertible {
  public float[] OffsetMatrix { get; } = new float[2 * 3];
  public sbyte ScaleExponent { get; set; }
  private readonly byte padding1_ = 0xff;
  private readonly byte padding2_ = 0xff;
  private readonly byte padding3_ = 0xff;
}

[BinarySchema]
public partial class IndTexCoordScale : IBinaryConvertible {
  public byte ScaleS { get; set; }
  public byte ScaleT { get; set; }
  private readonly ushort padding_ = ushort.MaxValue;
}

[BinarySchema]
public partial class TevIndirect : IBinaryConvertible {
  public byte TevStageId { get; set; }
  public byte IndTexFormat { get; set; }
  public byte IndTexBiasSel { get; set; }
  public byte IndTexMtdId { get; set; }
  public byte IndTexWrapS { get; set; }
  public byte IndTexWrapT { get; set; }
  public byte AddPrev { get; set; }
  public byte UtcLod { get; set; }
  public byte A { get; set; }
  private readonly byte padding1_ = byte.MaxValue;
  private readonly byte padding2_ = byte.MaxValue;
  private readonly byte padding3_ = byte.MaxValue;
}