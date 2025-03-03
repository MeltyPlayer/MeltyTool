using schema.binary;
using schema.binary.attributes;

namespace visceral.schema.str.content;

[BinarySchema]
public partial class UncompressedData : IContent {
  [RSequenceUntilEndOfStream]
  public byte[] Bytes { get; set; }
}