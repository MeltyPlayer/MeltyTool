using schema.binary;

namespace modl.schema.anim.bw2;

public class Bw2AnimBone : IBwAnimBone, IBinaryDeserializable {
  public string GetIdentifier() => this.Name;

  public string Name { get; set; }

  public uint PositionKeyframeCount { get; set; }
  public uint RotationKeyframeCount { get; set; }

  private readonly ulong padding0_ = 0;
  public float XPosDelta { get; set; }
  public float YPosDelta { get; set; }
  public float ZPosDelta { get; set; }
  public float XPosMin { get; set; }
  public float YPosMin { get; set; }
  public float ZPosMin { get; set; }
  private readonly uint padding1_ = 0;

  public void Read(IBinaryReader br) {
    this.Name = br.ReadString(16);

    this.PositionKeyframeCount = br.ReadUInt32();
    this.RotationKeyframeCount = br.ReadUInt32();

    br.AssertUInt64(0);

    this.XPosDelta = br.ReadSingle();
    this.YPosDelta = br.ReadSingle();
    this.ZPosDelta = br.ReadSingle();

    this.XPosMin = br.ReadSingle();
    this.YPosMin = br.ReadSingle();
    this.ZPosMin = br.ReadSingle();

    br.AssertUInt32(0);

    var values = br.ReadBytes(4);
  }
}