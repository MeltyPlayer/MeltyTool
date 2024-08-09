using schema.binary;
using schema.binary.attributes;

namespace dat.schema;

[Flags]
public enum RObjRefType : byte {
  EXP = 0,
  JOBJ = 1,
  LIMIT = 2,
  BYTECODE = 3,
  IKHINT = 4,
}

[Flags]
public enum RObjLimitType : byte {
  MIN_ROTX = 1,
  MAX_ROTX = 2,
  MIN_ROTY = 3,
  MAX_ROTY = 4,
  MIN_ROTZ = 5,
  MAX_ROTZ = 6,
  MIN_TRAX = 7,
  MAX_TRAX = 8,
  MIN_TRAY = 9,
  MAX_TRAY = 10,
  MIN_TRAZ = 11,
  MAX_TRAZ = 12,
}

/// <summary>
///   Reference object.
///
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/1a03d63a35376adc79a0a7495a389ea1a9dc4226/HSDRaw/Common/HSD_ROBJ.cs#L34
/// </summary>
[BinarySchema]
public partial class RObj : IDatLinkedListNode<RObj>, IBinaryDeserializable {
  public uint NextRObjOffset { get; set; }
  public int Flags { get; set; }
  public int ReferenceOffset { get; set; }


  [RAtPositionOrNull(nameof(NextRObjOffset))]
  public RObj? NextSibling { get; set; }

  [Skip]
  public RObjRefType RefType => (RObjRefType) (this.Flags >> 28);

  [Skip]
  public RObjLimitType LimitType => (RObjLimitType) (this.Flags & 0xFFFFFF);
}