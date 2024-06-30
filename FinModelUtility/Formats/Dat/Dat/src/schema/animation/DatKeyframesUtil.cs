using schema.binary;

namespace dat.schema.animation;

public class DatKeyframe {
  public required int Frame { get; init; }
  public required float IncomingValue { get; init; }
  public required float OutgoingValue { get; set; }
  public required float IncomingTangent { get; init; }
  public required float OutgoingTangent { get; set; }
}

public static class DatKeyframesUtil {
  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Tools/FOBJ_Player.cs#L132
  /// </summary> 
  public static void ReadKeyframes(
      IBinaryReader br,
      IDatKeyframes datKeyframes,
      LinkedList<DatKeyframe> keyframes) {
    if (datKeyframes.JointTrackType
        is (< JointTrackType.HSD_A_J_ROTX or > JointTrackType.HSD_A_J_ROTZ)
           and (< JointTrackType.HSD_A_J_TRAX
                or > JointTrackType.HSD_A_J_TRAZ)
           and (< JointTrackType.HSD_A_J_SCAX
                or > JointTrackType.HSD_A_J_SCAZ)) {
      return;
    }

    keyframes.Clear();

    var keys = ReadFObjKeys_(br, datKeyframes);
    var interpolations = GetInterpolationsFromFObjKeys_(keys);

    if (interpolations.Count == 0) {
      return;
    }

    DatKeyframe? currentKeyframe = null;

    var firstInterpolation = interpolations.First.Value;
    DatKeyframe nextKeyframe = new() {
        Frame = firstInterpolation.T0,
        IncomingValue = firstInterpolation.P0,
        OutgoingValue = firstInterpolation.P0,
        IncomingTangent = firstInterpolation.D0,
        OutgoingTangent = firstInterpolation.D0,
    };
    if (nextKeyframe.Frame >= 0) {
      keyframes.AddLast(nextKeyframe);
    }

    foreach (var interpolation in interpolations) {
      currentKeyframe = nextKeyframe;
      currentKeyframe.OutgoingValue = interpolation.P0;
      currentKeyframe.OutgoingTangent = interpolation.D0;

      nextKeyframe = new() {
          Frame = interpolation.T1,
          IncomingValue = interpolation.P1,
          OutgoingValue = interpolation.P1,
          IncomingTangent = interpolation.D1,
          OutgoingTangent = interpolation.D1,
      };
      if (nextKeyframe.Frame >= 0) {
        keyframes.AddLast(nextKeyframe);
      }
    }
  }

  private class InterpolationRegisters {
    public required float P0 { get; init; }
    public required float P1 { get; init; }
    public required float D0 { get; init; }
    public required float D1 { get; init; }
    public required int T0 { get; init; }
    public required int T1 { get; init; }
  }

  private static LinkedList<InterpolationRegisters>
      GetInterpolationsFromFObjKeys_(
          IReadOnlyList<FObjKey> keys) {
    var registers = new LinkedList<InterpolationRegisters>();

    float p0 = 0;
    float p1 = 0;
    float d0 = 0;
    float d1 = 0;
    int t0 = 0;
    int t1 = 0;
    var op_intrp = GxInterpolationType.Constant;
    var op = GxInterpolationType.Constant;

    foreach (var key in keys) {
      op_intrp = op;
      op = key.InterpolationType;

      var timeChanged = false;

      switch (op) {
        case GxInterpolationType.Constant:
        case GxInterpolationType.Linear:
          p0 = p1;
          p1 = key.Value;
          if (op_intrp != GxInterpolationType.Slp) {
            d0 = d1;
            d1 = 0;
          }

          t0 = t1;
          t1 = key.Frame;

          timeChanged = true;
          break;
        case GxInterpolationType.Spl0:
          p0 = p1;
          d0 = d1;
          p1 = key.Value;
          d1 = 0;
          t0 = t1;
          t1 = key.Frame;

          timeChanged = true;
          break;
        case GxInterpolationType.Spl:
          p0 = p1;
          p1 = key.Value;
          d0 = d1;
          d1 = key.Tangent;
          t0 = t1;
          t1 = key.Frame;

          timeChanged = true;
          break;
        case GxInterpolationType.Slp:
          d0 = d1;
          d1 = key.Tangent;
          break;
        case GxInterpolationType.Key:
          p1 = key.Value;
          p0 = key.Value;
          break;
      }

      if (!timeChanged) {
        registers.RemoveLast();
      }

      registers.AddLast(new InterpolationRegisters {
          P0 = p0,
          P1 = p1,
          D0 = d0,
          D1 = d1,
          T0 = t0,
          T1 = t1,
      });
    }

    return registers;
  }

  private class FObjKey {
    public required GxInterpolationType InterpolationType { get; init; }
    public required float Value { get; init; }
    public required int Frame { get; init; }
    public required float Tangent { get; init; }
  }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/Tools/FOBJ_Decoder.cs#L47
  /// </summary>
  private static IReadOnlyList<FObjKey> ReadFObjKeys_(
      IBinaryReader br,
      IDatKeyframes datKeyframes) {
    br.PushMemberEndianness(Endianness.LittleEndian);

    var valueScale = (uint) (1 << (datKeyframes.ValueFlag & 0x1F));
    var tangentScale = (uint) (1 << (datKeyframes.TangentFlag & 0x1F));

    var valueFormat = (GXAnimDataFormat) (datKeyframes.ValueFlag & 0xE0);
    var tangentFormat = (GXAnimDataFormat) (datKeyframes.TangentFlag & 0xE0);

    var keys = new List<FObjKey>();
    br.SubreadAt(
        datKeyframes.DataOffset,
        (int) datKeyframes.DataLength,
        sbr => {
          // TODO: Will probably need to do something else to handle this
          var clock = 0; //-datKeyframes.StartFrame;

          while (!sbr.Eof) {
            var type = ReadPacked_(sbr);
            var interpolation = (GxInterpolationType) (type & 0x0F);
            int numOfKey = (type >> 4) + 1;

            if (interpolation == GxInterpolationType.None) {
              break;
            }

            for (int i = 0; i < numOfKey; i++) {
              var value = 0f;
              var tan = 0f;
              var time = 0;

              switch (interpolation) {
                case GxInterpolationType.Constant:
                case GxInterpolationType.Linear:
                case GxInterpolationType.Spl0:
                  value = ParseFloat_(sbr, valueFormat, valueScale);
                  time = ReadPacked_(sbr);
                  break;
                case GxInterpolationType.Spl:
                  value = ParseFloat_(sbr, valueFormat, valueScale);
                  tan = ParseFloat_(sbr, tangentFormat, tangentScale);
                  time = ReadPacked_(sbr);
                  break;
                case GxInterpolationType.Slp:
                  tan = ParseFloat_(sbr, tangentFormat, tangentScale);
                  break;
                case GxInterpolationType.Key:
                  value = ParseFloat_(sbr, valueFormat, valueScale);
                  break;
                default:
                  throw new Exception("Unknown Interpolation Type " +
                                      interpolation.ToString("X"));
              }

              keys.Add(new FObjKey {
                  InterpolationType = interpolation,
                  Value = value,
                  Frame = clock,
                  Tangent = tan,
              });

              clock += time;
            }
          }
        });

    br.PopEndianness();

    return keys;
  }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/Ploaj/HSDLib/blob/master/HSDRaw/BinaryReaderExt.cs#L249C9-L259C10
  /// </summary>
  private static int ReadPacked_(IBinaryReader br) {
    int result = 0;
    int shift = 0;
    int parse;

    do {
      parse = br.ReadByte();
      result |= (parse & 0x7F) << shift;
      shift += 7;
    } while ((parse & 0x80) != 0);

    return result;
  }

  private static float ParseFloat_(IBinaryReader br,
                                   GXAnimDataFormat format,
                                   float scale)
    => format switch {
        GXAnimDataFormat.Float  => br.ReadSingle(),
        GXAnimDataFormat.Short  => br.ReadInt16() / scale,
        GXAnimDataFormat.UShort => br.ReadUInt16() / scale,
        GXAnimDataFormat.SByte  => br.ReadSByte() / scale,
        GXAnimDataFormat.Byte   => br.ReadByte() / scale,
        _                       => 0
    };
}