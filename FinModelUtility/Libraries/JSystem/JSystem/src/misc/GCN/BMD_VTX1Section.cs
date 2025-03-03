using System;
using System.Numerics;

using fin.color;
using fin.util.asserts;

using gx;
using gx.vertex;

using jsystem.G3D_Binary_File_Format;
using jsystem.schema.j3dgraph.bmd.vtx1;

using schema.binary;

#pragma warning disable CS8604


namespace jsystem.GCN;

public partial class BMD {
  public partial class VTX1Section {
    public IColor[][] Colors = new IColor[2][];
    public Vector2[][] TexCoords = new Vector2[8][];
    public const string Signature = "VTX1";
    public DataBlockHeader Header;
    public uint ArrayFormatOffset;
    public uint[] Offsets;
    public ArrayFormat[] ArrayFormats;
    public Vector3[] Positions;
    public Vector3[] Normals;
    public Vector3[] Tangents;
    public Vector3[] Binormals;

    public VTX1Section(IBinaryReader br, out bool OK) {
      long position1 = br.Position;
      bool OK1;
      this.Header = new DataBlockHeader(br, "VTX1", out OK1);
      if (!OK1) {
        OK = false;
      } else {
        this.ArrayFormatOffset = br.ReadUInt32();
        this.Offsets = br.ReadUInt32s(13);
        long position2 = br.Position;
        int length1 = 0;
        foreach (uint offset in this.Offsets) {
          if (offset != 0U)
            ++length1;
        }

        br.Position = position1 + (long) this.ArrayFormatOffset;
        this.ArrayFormats = br.ReadNews<ArrayFormat>(length1);

        int index1 = 0;
        for (int k = 0; k < 13; ++k) {
          if (this.Offsets[k] != 0U) {
            ArrayFormat arrayFormat = this.ArrayFormats[index1];
            int length2 = this.GetLength(k);
            br.Position = position1 + (long) this.Offsets[k];
            if (arrayFormat.ArrayType is GxVertexAttribute.Color0
                                         or GxVertexAttribute.Color1) {
              this.ReadColorArray(arrayFormat, length2, br);
            } else {
              this.ReadVertexArray(arrayFormat, length2, br);
            }

            ++index1;
          }
        }

        br.Position = position1 + (long) this.Header.size;
        OK = true;
      }
    }

    private int GetLength(int k) {
      int offset = (int) this.Offsets[k];
      for (int index = k + 1; index < 13; ++index) {
        if (this.Offsets[index] != 0U)
          return (int) this.Offsets[index] - offset;
      }

      return (int) this.Header.size - offset;
    }

    private void ReadVertexArray(
        ArrayFormat Format,
        int dataLength,
        IBinaryReader br) {
      var axisComponentType = (GxAxisComponentType) Format.DataType;
      var valueCount = dataLength / axisComponentType.GetByteCount();
      var componentCount = GxAttributeUtil.GetComponentCount(Format.ArrayType,
        Format.ComponentCountType);
      var vectorCount = valueCount / componentCount;

      switch (Format.ArrayType) {
        case GxVertexAttribute.Position: {
          this.Positions = new Vector3[vectorCount];
          for (var i = 0; i < vectorCount; ++i) {
            this.Positions[i] = GxAttributeUtil.ReadPosition(
                br,
                Format.ComponentCountType,
                axisComponentType,
                Format.DecimalPoint);
          }

          break;
        }
        case GxVertexAttribute.Normal: {
          this.Normals = new Vector3[vectorCount];
          for (var i = 0; i < vectorCount; ++i) {
            this.Normals[i] = GxAttributeUtil.ReadNormal(
                br,
                Format.ComponentCountType,
                axisComponentType,
                Format.DecimalPoint);
          }

          break;
        }
        case >= GxVertexAttribute.Tex0Coord
             and <= GxVertexAttribute.Tex7Coord: {
          var texCoordIndex = Format.ArrayType - GxVertexAttribute.Tex0Coord;
          var texCoords = this.TexCoords[texCoordIndex]
              = new Vector2[vectorCount];
          for (var i = 0; i < vectorCount; ++i) {
            texCoords[i] = GxAttributeUtil.ReadTexCoord(
                br,
                Format.ComponentCountType,
                axisComponentType,
                Format.DecimalPoint);
          }

          break;
        }
        default:
          throw new NotImplementedException();
      }
    }

    /// <summary>
    ///   Colors are a special case:
    ///   https://wiki.cloudmodding.com/tww/BMD_and_BDL#Data_Types
    /// </summary>
    private void ReadColorArray(
        ArrayFormat Format,
        int byteLength,
        IBinaryReader br) {
      var colorIndex = Format.ArrayType - GxVertexAttribute.Color0;

      var colorDataType = (GxColorComponentType) Format.DataType;
      var expectedComponentCount = colorDataType switch {
          GxColorComponentType.RGB565 => 3,
          GxColorComponentType.RGB8   => 3,
          GxColorComponentType.RGBX8  => 4,
          GxColorComponentType.RGBA4  => 4,
          GxColorComponentType.RGBA6  => 4,
          GxColorComponentType.RGBA8  => 4,
          _                           => throw new ArgumentOutOfRangeException()
      };

      var actualComponentCount
          = GxAttributeUtil.GetColorComponentCount(Format.ComponentCountType);
      Asserts.Equal(expectedComponentCount, actualComponentCount);

      var colorCount = colorDataType switch {
          GxColorComponentType.RGB565 => byteLength / 2,
          GxColorComponentType.RGB8   => byteLength / 3,
          GxColorComponentType.RGBX8  => byteLength / 4,
          GxColorComponentType.RGBA4  => byteLength / 2,
          GxColorComponentType.RGBA6  => byteLength / 3,
          GxColorComponentType.RGBA8  => byteLength / 4,
          _                           => throw new ArgumentOutOfRangeException()
      };

      var colors = this.Colors[colorIndex] = new IColor[colorCount];
      for (var i = 0; i < colorCount; ++i) {
        colors[i] = GxAttributeUtil.ReadColor(br, colorDataType);
      }
    }
  }
}