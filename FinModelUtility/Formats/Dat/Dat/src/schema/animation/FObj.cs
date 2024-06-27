using schema.binary;
using schema.binary.attributes;

namespace dat.schema.animation;

public enum GXAnimDataFormat {
  Float = 0x00,
  Short = 0x20,
  UShort = 0x40,
  SByte = 0x60,
  Byte = 0x80
}

public enum JointTrackType : byte {
  HSD_A_J_ROTX = 1, HSD_A_J_ROTY, HSD_A_J_ROTZ, HSD_A_J_PATH,
  HSD_A_J_TRAX, HSD_A_J_TRAY, HSD_A_J_TRAZ,
  HSD_A_J_SCAX, HSD_A_J_SCAY, HSD_A_J_SCAZ,
  HSD_A_J_NODE, HSD_A_J_BRANCH, HSD_A_J_SETBYTE0, HSD_A_J_SETBYTE1,
  HSD_A_J_SETBYTE2, HSD_A_J_SETBYTE3, HSD_A_J_SETBYTE4, HSD_A_J_SETBYTE5,
  HSD_A_J_SETBYTE6, HSD_A_J_SETBYTE7, HSD_A_J_SETBYTE8, HSD_A_J_SETBYTE9,
  HSD_A_J_SETFLOAT0, HSD_A_J_SETFLOAT1, HSD_A_J_SETFLOAT2, HSD_A_J_SETFLOAT3,
  HSD_A_J_SETFLOAT4, HSD_A_J_SETFLOAT5, HSD_A_J_SETFLOAT6, HSD_A_J_SETFLOAT7,
  HSD_A_J_SETFLOAT8, HSD_A_J_SETFLOAT9
}

public enum GxInterpolationType {
  None = 0,
  Constant = 1,
  Linear = 2,
  Spl0 = 3,
  Spl = 4,
  Slp = 5,
  Key = 6,
}

/// <summary>
///   Keyframe descriptor object.
/// 
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/b7554d5c753cca2d50090cdd7366afe64dd8f175/HSDRaw/Common/Animation/HSD_FOBJDesc.cs#L7
///   https://github.com/Ploaj/HSDLib/blob/b7554d5c753cca2d50090cdd7366afe64dd8f175/HSDRaw/Common/Animation/HSD_FOBJ.cs#L65
/// </summary>
[BinarySchema]
public partial class FObj : IDatLinkedListNode<FObj>,
                            IDatKeyframes,
                            IBinaryDeserializable {
  public uint NextSiblingOffset { get; set; }
  public uint DataLength { get; set; }
  public int StartFrame { get; set; }

  public JointTrackType JointTrackType { get; set; }
  public byte ValueFlag { get; set; }
  public byte TangentFlag { get; set; }
  public byte Unk { get; set; }
  public uint DataOffset { get; set; }


  [RAtPositionOrNull(nameof(NextSiblingOffset))]
  public FObj? NextSibling { get; set; }

  [Skip]
  public LinkedList<(int frame, float incomingValue, float outgoingValue, float? incomingTangent, float? outgoingTangent)> Keyframes {
    get;
  } = [];


  [ReadLogic]
  private void ReadKeyframes_(IBinaryReader br) {
      DatKeyframesUtil.ReadKeyframes(br, this, this.Keyframes);
    }
}