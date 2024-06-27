using schema.binary;
using schema.binary.attributes;

namespace dat.schema.animation;

[Flags]
public enum AOBJ_Flags : uint {
  ANIM_REWINDED = (1 << 26),
  FIRST_PLAY = (1 << 27),
  NO_UPDATE = (1 << 28),
  ANIM_LOOP = (1 << 29),
  NO_ANIM = (1 << 30)
}

/// <summary>
///   Animation object.
///
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/b7554d5c753cca2d50090cdd7366afe64dd8f175/HSDRaw/Common/Animation/HSD_AOBJ.cs#L18
/// </summary>
[BinarySchema]
public partial class AObj : IBinaryDeserializable {
  public AOBJ_Flags Flags { get; set; }
  public float EndFrame { get; set; }
  public uint FObjOffset { get; set; }
  public uint JObjOffset { get; set; }


  [RAtPositionOrNull(nameof(FObjOffset))]
  public FObj? FirstFObj { get; set; }

  [Skip]
  public IEnumerable<FObj> FObjs => this.FirstFObj.GetSelfAndSiblings();
}