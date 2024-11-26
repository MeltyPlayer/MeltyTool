using schema.binary;

namespace sysdolphin.schema.animation;

public class DatKeyframe {
  public int Frame { get; init; }
  public float IncomingValue { get; set; }
  public float OutgoingValue { get; set; }
  public float? IncomingTangent { get; set; }
  public float? OutgoingTangent { get; set; }
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
        Frame = firstInterpolation.FromFrame,
        IncomingValue = firstInterpolation.FromValue,
        OutgoingValue = firstInterpolation.FromValue,
        OutgoingTangent = null,
    };
    if (nextKeyframe.Frame >= 0) {
      keyframes.AddLast(nextKeyframe);
    }

    foreach (var interpolation in interpolations) {
      currentKeyframe = nextKeyframe;
      currentKeyframe.OutgoingValue = interpolation.FromValue;
      currentKeyframe.OutgoingTangent = interpolation.FromTangent;

      nextKeyframe = new() {
          Frame = interpolation.ToFrame,
          IncomingValue = interpolation.ToValue,
          IncomingTangent = interpolation.ToTangent,
          OutgoingValue = interpolation.ToValue,
      };

      if (nextKeyframe.Frame >= 0) {
        keyframes.AddLast(nextKeyframe);
      }

      if (interpolation.InterpolationType is GxInterpolationType
              .ConstantSection) {
        currentKeyframe.OutgoingValue
            = nextKeyframe.IncomingValue = interpolation.ToValue;
      }

      if (interpolation.InterpolationType is
          GxInterpolationType.ConstantSection
          or GxInterpolationType.LinearSection) {
        currentKeyframe.OutgoingTangent = nextKeyframe.IncomingTangent = null;
      }
    }
  }

  private class InterpolationRegisters {
    public required GxInterpolationType InterpolationType { get; init; }
    public required float FromValue { get; init; }
    public required float ToValue { get; init; }
    public required float FromTangent { get; init; }
    public required float ToTangent { get; init; }
    public required int FromFrame { get; init; }
    public required int ToFrame { get; init; }
  }

  /// <summary>
  ///   Helper method for getting the interpolations between each of the FObj
  ///   keyframes.
  /// </summary>
  private static LinkedList<InterpolationRegisters>
      GetInterpolationsFromFObjKeys_(
          IReadOnlyList<FObjKey> keys) {
    var registers = new LinkedList<InterpolationRegisters>();

    float fromValue = 0;
    float toValue = 0;
    float fromTangent = 0;
    float toTangent = 0;
    int fromFrame = 0;
    int toFrame = 0;

    foreach (var key in keys) {
      var interpolationType = key.InterpolationType;

      var timeChanged = false;

      switch (interpolationType) {
        case GxInterpolationType.ConstantSection:
          fromValue = toValue = key.Value;
          fromTangent = toTangent = 0;
          fromFrame = toFrame;
          toFrame = key.Frame;

          timeChanged = true;
          break;
        case GxInterpolationType.LinearSection:
          fromValue = toValue;
          toValue = key.Value;
          fromTangent = toTangent = 0;
          fromFrame = toFrame;
          toFrame = key.Frame;

          timeChanged = true;
          break;
        case GxInterpolationType.SplineTo0Section:
          fromValue = toValue;
          fromTangent = toTangent;
          toValue = key.Value;
          toTangent = 0;
          fromFrame = toFrame;
          toFrame = key.Frame;

          timeChanged = true;
          break;
        case GxInterpolationType.SplineSection:
          fromValue = toValue;
          toValue = key.Value;
          fromTangent = toTangent;
          toTangent = key.Tangent;
          fromFrame = toFrame;
          toFrame = key.Frame;

          timeChanged = true;
          break;
        case GxInterpolationType.FromTangentSetter:
          fromTangent = toTangent;
          toTangent = key.Tangent;
          break;
        case GxInterpolationType.FromValueSetter:
          toValue = key.Value;
          fromValue = key.Value;
          break;
      }

      if (timeChanged && fromFrame != toFrame) {
        registers.AddLast(new InterpolationRegisters {
            InterpolationType = interpolationType,
            FromValue = fromValue,
            ToValue = toValue,
            FromTangent = fromTangent,
            ToTangent = toTangent,
            FromFrame = fromFrame,
            ToFrame = toFrame,
        });
      }
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
  ///   Helper method for reading the FObj keyframes.
  /// 
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
        () => {
          // TODO: Will probably need to do something else to handle this
          var clock = 0; //-datKeyframes.StartFrame;

          while (!br.Eof) {
            var type = ReadPacked_(br);
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
                case GxInterpolationType.ConstantSection:
                case GxInterpolationType.LinearSection:
                case GxInterpolationType.SplineTo0Section:
                  value = ParseFloat_(br, valueFormat, valueScale);
                  time = ReadPacked_(br);
                  break;
                case GxInterpolationType.SplineSection:
                  value = ParseFloat_(br, valueFormat, valueScale);
                  tan = ParseFloat_(br, tangentFormat, tangentScale);
                  time = ReadPacked_(br);
                  break;
                case GxInterpolationType.FromTangentSetter:
                  tan = ParseFloat_(br, tangentFormat, tangentScale);
                  break;
                case GxInterpolationType.FromValueSetter:
                  value = ParseFloat_(br, valueFormat, valueScale);
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