using schema.binary;
using schema.binary.attributes;

namespace sysdolphin.schema.animation;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Common/Animation/HSD_Track.cs
/// </summary>
[BinarySchema]
public partial class FigaTreeTrack : IDatKeyframes, IBinaryDeserializable {
  [IntegerFormat(SchemaIntegerType.UINT16)]
  public uint DataLength { get; set; }

  [IntegerFormat(SchemaIntegerType.INT16)]
  public int StartFrame { get; set; }

  public byte TrackType { get; set; }

  public byte ValueFlag { get; set; }
  public byte TangentFlag { get; set; }

  public byte Unknown { get; set; }

  public uint DataOffset { get; set; }


  [Skip]
  public JointTrackType JointTrackType => (JointTrackType) this.TrackType;

  [Skip]
  public byte MatTrackType => this.TrackType;

  [Skip]
  public byte TexTrackType => this.TrackType;

  [Skip]
  public LinkedList<DatKeyframe> Keyframes { get; } = [];

  [ReadLogic]
  private void ReadKeyframes_(IBinaryReader br) {
    DatKeyframesUtil.ReadKeyframes(br, this, this.Keyframes);
  }
}