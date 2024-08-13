using fin.schema.data;

using jsystem.schema.jutility.bti;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd.tex1;

[BinarySchema]
[LocalPositions]
public partial class Tex1 : IBinaryConvertible {
  private readonly AutoStringMagicUInt32SizedSection<Tex1Data> impl_ =
      new("TEX1") {TweakReadSize = -8};

  [Skip]
  public Tex1Data Data => this.impl_.Data;
}

[BinarySchema]
public partial class Tex1Data : IBinaryConvertible {
  [WLengthOfSequence(nameof(TextureHeaders))]
  private ushort textureCount_;

  private readonly ushort padding_ = ushort.MaxValue;

  [WPointerTo(nameof(TextureHeaders))]
  private uint textureHeaderOffset_;

  [WPointerTo(nameof(StringTable))]
  private uint stringTableOffset_;

  [RSequenceLengthSource(nameof(textureCount_))]
  [RAtPosition(nameof(textureHeaderOffset_))]
  public Bti[] TextureHeaders;

  [RAtPosition(nameof(stringTableOffset_))]
  public readonly StringTable StringTable = new();
}