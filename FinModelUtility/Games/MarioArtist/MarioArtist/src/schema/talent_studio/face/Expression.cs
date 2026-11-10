using System.Numerics;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema.talent_studio.face;

[BinarySchema]
public partial class Expression : IBinaryDeserializable {
  public const int WIDTH = 9;
  public const int HEIGHT = 13;

  [SequenceLengthSource(WIDTH * HEIGHT)]
  public Vector2[] Pins { get; set; }
}