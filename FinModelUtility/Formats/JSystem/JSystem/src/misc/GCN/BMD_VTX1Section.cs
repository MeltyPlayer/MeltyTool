using System;
using System.Collections.Generic;
using System.Numerics;

using fin.util.asserts;
using fin.util.color;

using gx;
using gx.vertex;

using jsystem.G3D_Binary_File_Format;
using jsystem.schema.j3dgraph.bmd.vtx1;

using schema.binary;

#pragma warning disable CS8604


namespace jsystem.GCN;

public partial class BMD {
  public partial class VTX1Section {
    public Color[][] Colors = new Color[2][];
    public Texcoord[][] Texcoords = new Texcoord[8][];
    public const string Signature = "VTX1";
    public DataBlockHeader Header;
    public uint ArrayFormatOffset;
    public uint[] Offsets;
    public ArrayFormat[] ArrayFormats;
    public Vector3[] Positions;
    public Vector3[] Normals;

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
        int Length,
        IBinaryReader br) {
      List<float> floatList = [];
      switch ((GxAxisComponentType) Format.DataType) {
        case GxAxisComponentType.S16:
          float num1 = (float) Math.Pow(0.5, (double) Format.DecimalPoint);
          for (int index = 0; index < Length / 2; ++index)
            floatList.Add((float) br.ReadInt16() * num1);
          break;
        case GxAxisComponentType.F32:
          floatList.AddRange((IEnumerable<float>) br.ReadSingles(Length / 4));
          break;
        default:
          throw new NotImplementedException();
      }

      switch (Format.ArrayType) {
        case GxVertexAttribute.Position:
          switch (Format.ComponentCount) {
            case GxComponentCount.POS_XY:
              this.Positions = new Vector3[floatList.Count / 2];
              for (int index = 0; index < floatList.Count - 1; index += 2)
                this.Positions[index / 2] =
                    new Vector3(floatList[index], floatList[index + 1], 0.0f);
              return;
            case GxComponentCount.POS_XYZ:
              this.Positions = new Vector3[floatList.Count / 3];
              for (int index = 0; index < floatList.Count - 2; index += 3)
                this.Positions[index / 3] = new Vector3(
                    floatList[index],
                    floatList[index + 1],
                    floatList[index + 2]);
              return;
            default:
              return;
          }
        case GxVertexAttribute.Normal:
          if (Format.ComponentCount != 0U)
            break;
          this.Normals = new Vector3[floatList.Count / 3];
          for (int index = 0; index < floatList.Count - 2; index += 3)
            this.Normals[index / 3] = new Vector3(
                floatList[index],
                floatList[index + 1],
                floatList[index + 2]);
          break;
        case >= GxVertexAttribute.Tex0Coord and <= GxVertexAttribute.Tex7Coord:
          var texCoordIndex = Format.ArrayType - GxVertexAttribute.Tex0Coord;
          switch (Format.ComponentCount) {
            case GxComponentCount.TEX_S:
              this.Texcoords[texCoordIndex] = new Texcoord[floatList.Count];
              for (int index = 0; index < floatList.Count; ++index)
                this.Texcoords[texCoordIndex][index] =
                    new Texcoord(floatList[index], 0.0f);
              return;
            case GxComponentCount.TEX_ST:
              this.Texcoords[texCoordIndex] =
                  new Texcoord[floatList.Count / 2];
              for (int index = 0; index < floatList.Count - 1; index += 2)
                this.Texcoords[texCoordIndex][index / 2] =
                    new Texcoord(
                        floatList[index],
                        floatList[index + 1]);
              return;
            default:
              return;
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

      var actualComponentCount = (int) (3 + Format.ComponentCount);
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

      var colors = new Color[colorCount];
      for (var i = 0; i < colorCount; ++i) {
        Color color;
        switch (colorDataType) {
          case GxColorComponentType.RGB565: {
            ColorUtil.SplitRgb565(br.ReadUInt16(),
                                  out var r,
                                  out var g,
                                  out var b);
            color = new Color(r, g, b, 255);
            break;
          }
          case GxColorComponentType.RGB8: {
            color = new Color(br.ReadByte(),
                              br.ReadByte(),
                              br.ReadByte(),
                              255);
            break;
          }
          case GxColorComponentType.RGBX8: {
            color = new Color(br.ReadByte(),
                              br.ReadByte(),
                              br.ReadByte(),
                              br.ReadByte());
            break;
          }
          case GxColorComponentType.RGBA4: {
            throw new ArgumentOutOfRangeException();
          }
          case GxColorComponentType.RGBA6: {
            throw new ArgumentOutOfRangeException();
          }
          case GxColorComponentType.RGBA8: {
            color = new Color(br.ReadByte(),
                              br.ReadByte(),
                              br.ReadByte(),
                              br.ReadByte());
            break;
          }
          default: throw new ArgumentOutOfRangeException();
        }

        Asserts.Nonnull(color);

        colors[i] = color;
      }

      this.Colors[colorIndex] = colors;
    }

    public class Color(byte r, byte g, byte b, byte a) {
      public byte R = r;
      public byte G = g;
      public byte B = b;
      public byte A = a;

      public static implicit operator System.Drawing.Color(Color c) {
        return System.Drawing.Color.FromArgb((int) c.A,
                                             (int) c.R,
                                             (int) c.G,
                                             (int) c.B);
      }
    }

    public class Texcoord(float s, float t) {
      public float S = s;
      public float T = t;
    }
  }
}