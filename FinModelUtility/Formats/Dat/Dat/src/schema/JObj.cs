using fin.math.matrix.four;
using fin.schema;
using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace dat.schema;

[Flags]
public enum JObjFlags : uint {
  SKELETON = 1 << 0,
  SKELETON_ROOT = 1 << 1,
  ENVELOPE = 1 << 2,
  CLASSICAL_SCALE = 1 << 3,
  HIDDEN = 1 << 4,
  PTCL = 1 << 5,
  MTX_DIRTY = 1 << 6,
  LIGHTING = 1 << 7,
  TEXGEN = 1 << 8,

  // BILLBOARD
  BILLBOARD = 1 << 9,
  VBILLBOARD = 2 << 9,
  HBILLBOARD = 3 << 9,
  RBILLBOARD = 4 << 9,
  BILLBOARD_MASK = 0xF00,

  INSTANCE = 1 << 12,
  PBILLBOARD = 1 << 13,
  SPLINE = 1 << 14,
  FLIP_IK = 1 << 15,
  SPECULAR = 1 << 16,
  USE_QUATERNION = 1 << 17,

  // TRSP
  TRSP_OPA = 1 << 18,
  TRSP_XLU = 1 << 19,
  TRSP_TEXEDGE = 1 << 20,

  // TYPE
  TYPE_NULL = 0 << 21,
  TYPE_JOINT1 = 1 << 21,
  TYPE_JOINT2 = 2 << 21,
  TYPE_EFFECTOR = 3 << 21,

  USER_DEFINED_MTX = 1 << 23,
  MTX_INDEPEND_PARENT = 1 << 24,
  MTX_INDEPEND_SRT = 1 << 25,

  ROOT_OPA = 1 << 28,
  ROOT_XLU = 1 << 29,
  ROOT_TEXEDGE = 1 << 30,
}

/// <summary>
///   Joint object.
/// </summary>
[BinarySchema]
public partial class JObj : IDatTreeNode<JObj>, IBinaryDeserializable {
  public uint StringOffset { get; set; }
  public JObjFlags Flags { get; set; }
  public uint FirstChildBoneOffset { get; set; }
  public uint NextSiblingBoneOffset { get; set; }
  public uint FirstDObjOffset { get; set; }
  public Vector3f RotationRadians { get; } = new();
  public Vector3f Scale { get; } = new();
  public Vector3f Position { get; } = new();
  public uint InverseBindMatrixOffset { get; set; }

  [Unknown]
  public uint UnknownPointer { get; set; }


  [Skip]
  public string? Name { get; set; }

  [RAtPositionOrNull(nameof(FirstChildBoneOffset))]
  public JObj? FirstChild { get; set; }

  [RAtPositionOrNull(nameof(NextSiblingBoneOffset))]
  public JObj? NextSibling { get; set; }

  [RAtPositionOrNull(nameof(FirstDObjOffset))]
  public DObj? FirstDObj { get; set; }

  [RAtPositionOrNull(nameof(InverseBindMatrixOffset))]
  [SequenceLengthSource(4 * 3)]
  public float[]? InverseBindMatrixValues { get; set; }


  [Skip]
  public IEnumerable<DObj> DObjs => this.FirstDObj.GetSelfAndSiblings();

  public override string? ToString() => this.Name;
}