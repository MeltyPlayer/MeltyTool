using fin.util.strings;

using grezzo.schema.cmb;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.ctxb;

[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public partial class Ctxb : IBinaryConvertible {
  public CtxbHeader Header { get; } = new();
  public CtxbTexChunk Chunk { get; } = new();
}

[BinarySchema]
public partial class CtxbHeader : IChildOf<Ctxb>, IBinaryConvertible {
  public Ctxb Parent { get; set; }

  private readonly string magic_ = "ctxb";

  [WSizeOfStreamInBytes]
  public int CtxbSize { get; private set; }

  private readonly uint texCount_ = 1;
  private readonly uint padding_ = 0;

  [WPointerTo(nameof(Parent.Chunk))]
  public int ChunkOffset { get; private set; }

  [WPointerTo(nameof(Parent.Chunk.Entry.Data))]
  public int DataOffset { get; private set; }
}

[BinarySchema]
public partial class CtxbTexChunk : IBinaryConvertible {
  private readonly string magic_ = "tex" + AsciiUtil.GetChar(0x20);
  private readonly int chunkSize_ = 0x30;

  private readonly uint texCount_ = 1;

  public CtxbTexEntry Entry { get; } = new();
}

[BinarySchema]
public partial class CtxbTexEntry : IBinaryConvertible {
  [WLengthOfSequence(nameof(Data))]
  private uint dataLength_;
  public ushort mimapCount { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool IsEtc1 { get; private set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool IsCubemap { get; private set; }

  public ushort Width { get; private set; }
  public ushort Height { get; private set; }
  public GlTextureFormat ImageFormat { get; private set; }

  [StringLengthSource(16)]
  public string Name { get; private set; }

  private readonly uint padding_ = 0;

  [Skip]
  private bool includeExtraPadding_ 
    => CmbHeader.Version >= Version.LUIGIS_MANSION_3D;

  [RIfBoolean(nameof(includeExtraPadding_))]
  [SequenceLengthSource(56)]
  private byte[]? extraPadding_;

  [RSequenceLengthSource(nameof(dataLength_))]
  public byte[] Data { get; private set; }
}