using System.Numerics;

using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

using sonicadventure.util;

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

/// <summary>
///   Shamelessly stolen from:
///   https://info.sonicretro.org/SCHG:Sonic_Adventure/Model_Format
/// </summary>
[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public partial class Object(uint keyedPointer, uint key) : IKeyedInstance<Object> {
  public static Object New(uint keyedPointer, uint key) => new(keyedPointer, key);

  public ObjectFlags Flags { get; set; }
  private uint attachPointer_;
  
  public Vector3 Position { get; set; }
  public Vector3i Rotation { get; } = new();
  public Vector3 Scale { get; set; }

  private uint childPointer_;
  private uint relatePointer_;

  [Skip]
  public Attach? Attach { get; set; }

  [Skip]
  public Object? FirstChild { get; set; }

  [Skip]
  public Object? NextSibling { get; set; }

  [ReadLogic]
  private void ReadObjects_(IBinaryReader br) {
    this.Attach = br.ReadAtPointerOrNull<Attach>(this.attachPointer_, key);
    this.FirstChild = br.ReadAtPointerOrNull<Object>(this.childPointer_, key);
    this.NextSibling = br.ReadAtPointerOrNull<Object>(this.relatePointer_, key);
  }
}