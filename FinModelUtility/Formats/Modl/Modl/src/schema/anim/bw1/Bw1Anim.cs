using fin.util.asserts;

using schema.binary;

namespace modl.schema.anim.bw1;

public class Bw1Anim : IAnim, IBinaryDeserializable {
  public List<IBwAnimBone> AnimBones { get; private set; }
  public List<AnimBoneFrames> AnimBoneFrames { get; private set; }

  public void Read(IBinaryReader br) {
    string name0;
    {
      br.PushMemberEndianness(Endianness.LittleEndian);
      var name0Length = br.ReadUInt32();
      name0 = br.ReadString((int) name0Length);
      br.PopEndianness();
    }

    var animStart = br.Position;

    var single0 = br.ReadSingle();
    var uint0 = br.ReadUInt32();

    // The name is repeated once more, excepted null-terminated.
    var name1 = br.ReadStringNT();

    // Next is a series of many "0xcd" values. Why??
    var cdCount = 0;
    while (br.ReadByte() == 0xcd) {
      cdCount++;
    }

    --br.Position;

    var single1 = br.ReadSingle();

    Asserts.Equal(0x4c, br.Position - animStart);

    // Next is a series of bone definitions. Each one has a length of 64.
    var boneCount = br.ReadUInt32();

    br.ReadUInt32s(2);

    this.AnimBones = new List<IBwAnimBone>((int) boneCount);
    this.AnimBoneFrames = new List<AnimBoneFrames>((int) boneCount);
    for (var i = 0; i < boneCount; ++i) {
      var bone = new Bw1AnimBone();
      bone.Read(br);
      this.AnimBones.Add(bone);
    }

    // Next is blocks of data separated by "0xcd" bytes. The lengths of these can be predicted with the ints in the bones, so these seem to be keyframes.
    var estimatedLengths = this.AnimBones
                               .Select(
                                   bone =>
                                       bone.PositionKeyframeCount * 4 +
                                       bone.RotationKeyframeCount * 6)
                               .ToArray();

    // TODO: Remove this list allocation
    var boneBytes = new List<byte[]>();
    for (var i = 0; i < this.AnimBones.Count; ++i) {
      var currentBuffer = br.ReadBytes((int) estimatedLengths[i]);
      boneBytes.Add(currentBuffer);

      if (br.ReadUInt16() != 0xcdcd) {
        br.Position -= 2;
      }
    }

    for (var i = 0; i < boneCount; ++i) {
      var bone = this.AnimBones[i];

      var buffer = boneBytes[i];
      using var ber =
          new SchemaBinaryReader(buffer, Endianness.BigEndian);

      var animBoneFrames = new AnimBoneFrames(
          (int) bone.PositionKeyframeCount,
          (int) bone.RotationKeyframeCount);
      this.AnimBoneFrames.Add(animBoneFrames);

      Span<ushort> shorts = stackalloc ushort[3];

      for (var p = 0; p < bone.PositionKeyframeCount; ++p) {
        this.Parse3PositionValuesFrom2UShorts_(bone,
                                               ber,
                                               out var outX,
                                               out var outY,
                                               out var outZ);
        animBoneFrames.PositionFrames.Add(((float) outX,
                                           (float) outY,
                                           (float) outZ));
      }

      for (var p = 0; p < bone.RotationKeyframeCount; ++p) {
        var flipSigns = this.Parse4RotationValuesFrom3UShorts_(
                ber,
                shorts,
                out var outX,
                out var outY,
                out var outZ,
                out var outW);
        if (flipSigns) {
          outX *= -1;
          outY *= -1;
          outZ *= -1;
          outW *= -1;
        }

        animBoneFrames.RotationFrames.Add(((float) -outX,
                                           (float) -outY,
                                           (float) -outZ,
                                           (float) outW));
      }
    }
  }

  public void Parse3PositionValuesFrom2UShorts_(
      IBwAnimBone animBone,
      SchemaBinaryReader br,
      out double outX,
      out double outY,
      out double outZ) {
    var first_uint = br.ReadUInt32();
    br.Position -= 2;
    var second_ushort = br.ReadUInt16();

    outX =
        WeirdFloatMath.CreateWeirdDoubleFromUInt32(first_uint >> 0x15) *
        animBone.XPosDelta + animBone.XPosMin;
    outY =
        WeirdFloatMath.CreateWeirdDoubleFromUInt32(
            (first_uint >> 10) & 0x7ff) * animBone.YPosDelta +
        animBone.YPosMin;
    outZ =
        WeirdFloatMath.CreateWeirdDoubleFromUInt32(
            (uint) (second_ushort & 0x3ff)) * animBone.ZPosDelta +
        animBone.ZPosMin;
  }

  public bool Parse4RotationValuesFrom3UShorts_(IBinaryReader br,
                                                Span<ushort> shorts,
                                                out double outX,
                                                out double outY,
                                                out double outZ,
                                                out double outW) {
    br.ReadUInt16s(shorts);

    var first_ushort = shorts[0];
    var second_ushort = shorts[1];
    var third_ushort = shorts[2];

    outX =
        (WeirdFloatMath.CreateWeirdDoubleFromUInt32(
             (uint) (first_ushort & 0x7fff)) -
         WeirdFloatMath.C_16384) *
        WeirdFloatMath.C_6_10351_EN5;

    outY =
        (WeirdFloatMath.CreateWeirdDoubleFromUInt32(
             (uint) (second_ushort & 0x7fff)) -
         WeirdFloatMath.C_16384) *
        WeirdFloatMath.C_6_10351_EN5;

    outZ =
        (WeirdFloatMath.CreateWeirdDoubleFromUInt32(third_ushort) -
         32768f) *
        WeirdFloatMath.C_3_05175_EN5;

    var expected_normalized_w =
        ((1 - outX * outX) - outY * outY) - outZ * outZ;
    outW = 0d;
    if (expected_normalized_w > 0) {
      outW = Math.Sqrt(expected_normalized_w);

      var sign = first_ushort >> 0xf == 0 ? 1 : -1;
      outW *= sign;
    }

    return (short) second_ushort < 0;
  }
}