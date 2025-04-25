using schema.binary;
using schema.binary.attributes;

namespace sonicadventure.schema.model;

public interface IPoly : IBinaryConvertible;

[BinarySchema]
public partial class TrianglesPoly : IPoly {
  [SequenceLengthSource(3)]
  public ushort[] VertexIndices { get; private set; }
}

[BinarySchema]
public partial class QuadsPoly : IPoly {
  [SequenceLengthSource(4)]
  public ushort[] VertexIndices { get; private set; }
}

[BinarySchema]
public partial class TriangleStripPoly : IPoly {
  [WLengthOfSequence(nameof(VertexIndices))]
  public byte NumStrips { get; set; }

  public byte Direction { get; set; }

  [RSequenceLengthSource(nameof(NumStrips))]
  public ushort[] VertexIndices { get; set; }
}