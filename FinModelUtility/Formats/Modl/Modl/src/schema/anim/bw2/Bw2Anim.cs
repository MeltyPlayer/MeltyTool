using fin.util.asserts;

using schema.binary;

namespace modl.schema.anim.bw2;

public class Bw2Anim : IAnim, IBinaryDeserializable {
  public List<IBwAnimBone> AnimBones { get; } = [];
  public List<AnimBoneFrames> AnimBoneFrames { get; } = [];

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

    for (var i = 0; i < boneCount; ++i) {
      var bone = new Bw2AnimBone();
      bone.Read(br);
      this.AnimBones.Add(bone);
    }

    // Next is blocks of data separated by "0xcd" bytes. The lengths of these can be predicted with the ints in the bones, so these seem to be keyframes.
    var estimatedLengths = this.AnimBones
                               .Select(
                                   bone =>
                                       bone.PositionKeyframeCount * 2 * 2 +
                                       bone.RotationKeyframeCount * 2 * 4)
                               .ToArray();

    var totalLength = (uint) 0;
    foreach (var estimatedLength in estimatedLengths) {
      totalLength += estimatedLength;
    }

    var boneBytes = new List<byte[]>();
    for (var i = 0; i < this.AnimBones.Count; ++i) {
      var currentBuffer = br.ReadBytes((int) estimatedLengths[i]);
      boneBytes.Add(currentBuffer);

      // TODO: May no longer be necessary
      if (i + 1 < this.AnimBones.Count) {
        if (br.ReadUInt16() != 0xcdcd) {
          br.Position -= 2;
        }
      }
    }

    for (var i = 0; i < boneCount; ++i) {
      var bone = this.AnimBones[i];

      var buffer = boneBytes[i];
      using var ber =
          new SchemaBinaryReader(buffer, Endianness.BigEndian);

      var animBoneFrames = new AnimBoneFrames();
      this.AnimBoneFrames.Add(animBoneFrames);

      Span<double> floats = stackalloc double[4];

      for (var p = 0; p < bone.PositionKeyframeCount; ++p) {
        Parse3PositionValuesFrom2UShorts_(bone, ber, floats);
        animBoneFrames.PositionFrames.Add(((float) floats[0],
                                           (float) floats[1],
                                           (float) floats[2]));
      }

      for (var p = 0; p < bone.RotationKeyframeCount; ++p) {
        var flipSigns =
            this.Parse4RotationValuesFrom4UShorts_(ber, floats);
        if (flipSigns) {
          for (var f = 0; f < floats.Length; f++) {
            floats[f] *= -1;
          }
        }

        animBoneFrames.RotationFrames.Add(((float) -floats[0],
                                           (float) -floats[1],
                                           (float) -floats[2],
                                           (float) floats[3]));
      }
    }
  }

  public void Parse3PositionValuesFrom2UShorts_(
      IBwAnimBone animBone,
      SchemaBinaryReader br,
      Span<double> outValues) {
    var first_uint = br.ReadUInt32();
    br.Position -= 2;
    var second_ushort = br.ReadUInt16();

    outValues[0] =
        (WeirdFloatMath.InterpretAsDouble(
             WeirdFloatMath.Concat44(0x43300000,
                                     (uint) (first_uint >> 0x15))) -
         WeirdFloatMath.C_4503599627370496) *
        animBone.XPosDelta + animBone.XPosMin;
    outValues[1] =
        (WeirdFloatMath.InterpretAsDouble(
             WeirdFloatMath.Concat44(0x43300000,
                                     (uint) ((first_uint >> 10) & 0x7ff))) -
         WeirdFloatMath.C_4503599627370496) *
        animBone.YPosDelta + animBone.YPosMin;
    outValues[2] =
        (WeirdFloatMath.InterpretAsDouble(
             WeirdFloatMath.Concat44(0x43300000,
                                     (uint) (second_ushort & 0x3ff))) -
         WeirdFloatMath.C_4503599627370496) *
        animBone.ZPosDelta + animBone.ZPosMin;
  }

  public bool Parse4RotationValuesFrom4UShorts_(IBinaryReader br,
                                                Span<double> outValues) {
    Span<ushort> shorts = stackalloc ushort[4];
    br.ReadUInt16s(shorts);

    var first_ushort = shorts[0];
    var second_ushort = shorts[1];
    var third_ushort = shorts[2];
    var fourth_ushort = shorts[3];

    const double DOUBLE_80600f40 = 4.503601774854144E15;
    const double FLOAT_80603708 = 3.0517578E-5;

    var outX = (float) (WeirdFloatMath.InterpretAsDouble(
                            WeirdFloatMath.Concat44(0x43300000,
                                                    ((first_ushort & 0x3fffU) <<
                                                        1) ^
                                                    0x80000000)) -
                        DOUBLE_80600f40) * FLOAT_80603708;
    var outY = (float) (WeirdFloatMath.InterpretAsDouble(
                            WeirdFloatMath.Concat44(0x43300000,
                                                    (uint) ((second_ushort &
                                                            0x3fff) <<
                                                        1) ^
                                                    0x80000000)) -
                        DOUBLE_80600f40)
               * FLOAT_80603708;
    var outZ = (float) (WeirdFloatMath.InterpretAsDouble(
                            WeirdFloatMath.Concat44(0x43300000,
                                                    third_ushort & 0x7fffU ^
                                                    0x80000000)) -
                        DOUBLE_80600f40) * FLOAT_80603708;
    var outW = (float) (WeirdFloatMath.InterpretAsDouble(
                            WeirdFloatMath.Concat44(0x43300000,
                                                    fourth_ushort & 0x7fffU ^
                                                    0x80000000)) -
                        DOUBLE_80600f40) * FLOAT_80603708;
    if (((int) first_ushort & 0x4000U) != 0) {
      outX = -outX;
    }

    if (((int) (short) (second_ushort << 1) & 0x8000U) != 0) {
      outY = -outY;
    }

    if (((int) third_ushort & 0x8000U) != 0) {
      outZ = -outZ;
    }

    if (((int) fourth_ushort & 0x8000U) != 0) {
      outW = -outW;
    }

    outValues[0] = outX;
    outValues[1] = outY;
    outValues[2] = outZ;
    outValues[3] = outW;

    return (-(second_ushort >> 0xf & 1) >> 0x1f) != 0;
  }
}