using System.Numerics;

using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace sonicadventure.schema.model;

[Flags]
public enum ObjectFlags : uint {
  DISABLE_POSITION = 1 << 0,
  DISABLE_ROTATION = 1 << 1,
  DISABLE_SCALE = 1 << 2,
  DISABLE_RENDERING = 1 << 3,
  DISABLE_CHILDREN = 1 << 4,
  ZYX_ROTATIONS = 1 << 5,
  DISABLE_ANIMATIONS = 1 << 6,
  DISABLE_MORPHS = 1 << 7,
}

[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public partial class Object : IBinaryDeserializable {
  public ObjectFlags Flags { get; set; }
  private uint attachPointer_;
  private Vector3 Position { get; set; }
  private Vector3i Rotation { get; set; }
  private Vector3 Scale { get; set; }
  private uint childPointer_;
  private uint relatePointer_;

  [RAtPositionOrNull(nameof(attachPointer_))]
  public Attach? Attach { get; set; }

  [RAtPositionOrNull(nameof(childPointer_))]
  public Object? Child { get; set; }

  [RAtPositionOrNull(nameof(relatePointer_))]
  public Object? Relate { get; set; }
}