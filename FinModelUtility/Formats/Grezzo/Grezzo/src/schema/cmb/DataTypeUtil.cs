using System;

using schema.binary;

namespace grezzo.schema.cmb {
  public static class DataTypeUtil {
    // TODO: Is there a better way to read arbitrary data types?
    public static float Read(
        IBinaryReader br,
        DataType dataType)
      => dataType switch {
          DataType.Byte   => br.ReadSByte(),
          DataType.UByte  => br.ReadByte(),
          DataType.Short  => br.ReadInt16(),
          DataType.UShort => br.ReadUInt16(),
          DataType.Int    => br.ReadInt32(),
          DataType.UInt   => br.ReadUInt32(),
          DataType.Float  => br.ReadSingle(),
          _ => throw new ArgumentOutOfRangeException(
                   nameof(dataType),
                   dataType,
                   null)
      };

    public static float[] Read(
        IBinaryReader br,
        int count,
        DataType dataType) {
      var values = new float[count];

      for (var i = 0; i < count; ++i) {
        values[i] = DataTypeUtil.Read(br, dataType);
      }

      return values;
    }

    public static int GetSize(DataType dataType)
      => dataType switch {
          DataType.Byte   => 1,
          DataType.UByte  => 1,
          DataType.Short  => 2,
          DataType.UShort => 2,
          DataType.Int    => 4,
          DataType.UInt   => 4,
          DataType.Float  => 4,
          _ => throw new ArgumentOutOfRangeException(
                   nameof(dataType),
                   dataType,
                   null)
      };
  }
}