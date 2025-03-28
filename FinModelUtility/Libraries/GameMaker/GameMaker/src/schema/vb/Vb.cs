using System.Numerics;

using schema.binary;
using schema.binary.attributes;


namespace gm.schema.vb;

[BinarySchema]
public partial class Vb : IBinaryConvertible {
  [RSequenceUntilEndOfStream]
  public VbVertex[] Vertices { get; set; }
}

[BinarySchema]
public partial struct VbVertex : IBinaryConvertible {
  public Vector3 Position { get; set; }
  public Vector2 Uv { get; set; }
  public uint Unk { get; set; }
}