using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

using grezzo.schema.cmb.sklm;
using grezzo.schema.cmb.vatr;

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

    public static float Read(
        ReadOnlyMemory<byte> data,
        DataType dataType)
      => dataType switch {
          DataType.Byte   => (sbyte) data.Span[0],
          DataType.UByte  => data.Span[0],
          DataType.Short  => BinaryPrimitives.ReadInt16LittleEndian(data.Span),
          DataType.UShort => BinaryPrimitives.ReadUInt16LittleEndian(data.Span),
          DataType.Int    => BinaryPrimitives.ReadInt32LittleEndian(data.Span),
          DataType.UInt   => BinaryPrimitives.ReadUInt32LittleEndian(data.Span),
          DataType.Float  => BinaryPrimitives.ReadSingleLittleEndian(data.Span),
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

    public static IEnumerable<float> Read(
        AttributeSlice slice,
        VertexAttribute attribute,
        int itemsPerTuple) {
      if (attribute.Mode == VertexAttributeMode.Constant) {
        foreach (var constant in attribute.Constants.Take(itemsPerTuple)) {
          yield return constant;
        }

        yield break;
      }

      var span = slice.Bytes.AsMemory((int) attribute.Start);

      var dataType = attribute.DataType;
      var size = dataType.GetSize();
      var scale = attribute.Scale;

      for (var i = 0; i < span.Length / size; ++i) {
        yield return scale * DataTypeUtil.Read(span.Slice(size * i, size), dataType);
      }
    }

    public static int GetSize(this DataType dataType)
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