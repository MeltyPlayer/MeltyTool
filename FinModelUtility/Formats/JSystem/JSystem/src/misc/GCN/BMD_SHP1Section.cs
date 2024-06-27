using System;
using System.Collections.Generic;
using System.Linq;

using fin.schema;

using gx;

using jsystem.G3D_Binary_File_Format;
using jsystem.schema.j3dgraph.bmd.shp1;

using schema.binary;

#pragma warning disable CS8604

namespace jsystem.GCN;

public partial class BMD {
  public partial class SHP1Section {
    public const string Signature = "SHP1";
    public DataBlockHeader Header;
    public ushort NrBatch;
    public ushort Padding;
    public uint BatchesOffset;
    public uint ShapeRemapTableOffset;
    public short[] ShapeRemapTable;
    public uint Zero;
    public uint BatchAttribsOffset;
    public uint MatrixTableOffset;
    public uint DataOffset;
    public uint MatrixDataOffset;
    public uint PacketLocationsOffset;
    public Batch[] Batches;

    public SHP1Section(IBinaryReader br, out bool OK) {
      long position1 = br.Position;
      bool OK1;
      this.Header = new DataBlockHeader(br, "SHP1", out OK1);
      if (!OK1) {
        OK = false;
      } else {
        this.NrBatch = br.ReadUInt16();
        this.Padding = br.ReadUInt16();
        this.BatchesOffset = br.ReadUInt32();
        this.ShapeRemapTableOffset = br.ReadUInt32();
        this.Zero = br.ReadUInt32();
        this.BatchAttribsOffset = br.ReadUInt32();
        this.MatrixTableOffset = br.ReadUInt32();
        this.DataOffset = br.ReadUInt32();
        this.MatrixDataOffset = br.ReadUInt32();
        this.PacketLocationsOffset = br.ReadUInt32();
        long position2 = br.Position;
        {
          br.Position = position1 + (long) this.BatchesOffset;
          this.Batches = new Batch[(int) this.NrBatch];
          for (int index = 0; index < (int) this.NrBatch; ++index) {
            this.Batches[index] = new Batch(br, position1, this);
          }
        }
        {
          br.Position = position1 + (long) this.ShapeRemapTableOffset;
          this.ShapeRemapTable = br.ReadInt16s(this.NrBatch);
        }
        br.Position = position1 + (long) this.Header.size;
        OK = true;
      }
    }

    public enum MatrixType : byte {
      Mtx = 0,
      BBoard = 1,
      YBBoard = 2,
      Multi = 3,
    }

    public partial class Batch {
      public bool[] HasColors = new bool[2];
      public bool[] HasTexCoords = new bool[8];
      public MatrixType MatrixType;

      [Unknown]
      public byte Unknown1;

      public ushort NrPacket;
      public ushort AttribsOffset;
      public ushort FirstMatrixData;
      public ushort FirstPacketLocation;

      [Unknown]
      public ushort Unknown2;

      public float BoundingSphereReadius;
      public float[] BoundingBoxMin;
      public float[] BoundingBoxMax;
      public BatchAttribute[] BatchAttributes;
      public bool HasMatrixIndices;
      public bool HasPositions;
      public bool HasNormals;
      public PacketLocation[] PacketLocations;
      public Packet[] Packets;

      public Batch(
          IBinaryReader br,
          long baseoffset,
          SHP1Section Parent) {
        this.MatrixType = (MatrixType) br.ReadByte();
        this.Unknown1 = br.ReadByte();
        this.NrPacket = br.ReadUInt16();
        this.AttribsOffset = br.ReadUInt16();
        this.FirstMatrixData = br.ReadUInt16();
        this.FirstPacketLocation = br.ReadUInt16();
        this.Unknown2 = br.ReadUInt16();
        this.BoundingSphereReadius = br.ReadSingle();
        this.BoundingBoxMin = br.ReadSingles(3);
        this.BoundingBoxMax = br.ReadSingles(3);
        long position = br.Position;
        br.Position = baseoffset +
                      (long) Parent.BatchAttribsOffset +
                      (long) this.AttribsOffset;
        List<BatchAttribute> source = [];
        {
          BatchAttribute entry;
          do {
            entry = br.ReadNew<BatchAttribute>();
            source.Add(entry);
          } while ((uint) entry.Attribute != byte.MaxValue);
        }

        source.Remove(source.Last<BatchAttribute>());
        this.BatchAttributes = source.ToArray();
        for (int index = 0; index < this.BatchAttributes.Length; ++index) {
          if (this.BatchAttributes[index].DataType != 1U &&
              this.BatchAttributes[index].DataType != 3U)
            throw new Exception();
          switch (this.BatchAttributes[index].Attribute) {
            case GxAttribute.PNMTXIDX:
              this.HasMatrixIndices = true;
              break;
            case GxAttribute.POS:
              this.HasPositions = true;
              break;
            case GxAttribute.NRM:
              this.HasNormals = true;
              break;
            case GxAttribute.CLR0:
            case GxAttribute.CLR1:
              this.HasColors[
                      this.BatchAttributes[index].Attribute -
                      GxAttribute.CLR0] =
                  true;
              break;
            case GxAttribute.TEX0:
            case GxAttribute.TEX1:
            case GxAttribute.TEX2:
            case GxAttribute.TEX3:
            case GxAttribute.TEX4:
            case GxAttribute.TEX5:
            case GxAttribute.TEX6:
            case GxAttribute.TEX7:
              this.HasTexCoords[this.BatchAttributes[index].Attribute -
                                GxAttribute.TEX0] =
                  true;
              break;
          }
        }

        this.Packets = new Packet[(int) this.NrPacket];
        this.PacketLocations = new PacketLocation[(int) this.NrPacket];
        for (int index = 0; index < (int) this.NrPacket; ++index) {
          br.Position = baseoffset + (long) Parent.PacketLocationsOffset +
                        (long) (((int) this.FirstPacketLocation + index) * 8);
          var packetLocation = new PacketLocation();
          packetLocation.Read(br);
          this.PacketLocations[index] = packetLocation;

          br.Position = baseoffset + (long) Parent.DataOffset +
                        (long) this.PacketLocations[index].Offset;
          this.Packets[index] = new Packet(br,
                                           (int) this.PacketLocations[index]
                                               .Size,
                                           this.BatchAttributes);
          br.Position = baseoffset + (long) Parent.MatrixDataOffset +
                        (long) (((int) this.FirstMatrixData + index) * 8);
          this.Packets[index].MatrixData = br.ReadNew<MatrixData>();
          br.Position = baseoffset + (long) Parent.MatrixTableOffset +
                        (long) (2U * this.Packets[index].MatrixData
                                         .FirstIndex);
          this.Packets[index].MatrixTable =
              br.ReadUInt16s((int) this.Packets[index].MatrixData.Count);
        }

        br.Position = position;
      }

      public class Packet {
        public Primitive[] Primitives;
        public ushort[] MatrixTable;
        public MatrixData MatrixData;

        public Packet(
            IBinaryReader br,
            int Length,
            BatchAttribute[] Attributes) {
          List<Primitive> primitiveList = [];
          bool flag = false;
          int num1 = 0;
          while (!flag) {
            Primitive primitive = new Primitive();
            primitive.Type = (Primitive.GXPrimitive) br.ReadByte();
            ++num1;
            if (primitive.Type == (Primitive.GXPrimitive) 0 ||
                num1 >= Length) {
              flag = true;
            } else {
              ushort num2 = br.ReadUInt16();
              num1 += 2;
              primitive.Points = new Primitive.Index[(int) num2];
              for (int index1 = 0; index1 < (int) num2; ++index1) {
                primitive.Points[index1] = new Primitive.Index();
                for (int index2 = 0; index2 < Attributes.Length; ++index2) {
                  ushort num3 = 0;
                  switch (Attributes[index2].DataType) {
                    case 1:
                      num3 = (ushort) br.ReadByte();
                      ++num1;
                      break;
                    case 3:
                      num3 = br.ReadUInt16();
                      num1 += 2;
                      break;
                  }

                  switch (Attributes[index2].Attribute) {
                    case GxAttribute.PNMTXIDX:
                      primitive.Points[index1].MatrixIndex = num3;
                      break;
                    case GxAttribute.POS:
                      primitive.Points[index1].PosIndex = num3;
                      break;
                    case GxAttribute.NRM:
                      primitive.Points[index1].NormalIndex = num3;
                      break;
                    case GxAttribute.CLR0:
                    case GxAttribute.CLR1:
                      primitive.Points[index1]
                               .ColorIndex[
                                   (Attributes[index2].Attribute -
                                    GxAttribute.CLR0)] = num3;
                      break;
                    case GxAttribute.TEX0:
                    case GxAttribute.TEX1:
                    case GxAttribute.TEX2:
                    case GxAttribute.TEX3:
                    case GxAttribute.TEX4:
                    case GxAttribute.TEX5:
                    case GxAttribute.TEX6:
                    case GxAttribute.TEX7:
                      primitive.Points[index1]
                               .TexCoordIndex[
                                   (Attributes[index2].Attribute -
                                    GxAttribute.TEX0)] = num3;
                      break;
                  }
                }
              }

              primitiveList.Add(primitive);
            }
          }

          this.Primitives = primitiveList.ToArray();
        }

        public class Primitive {
          public GXPrimitive Type;
          public Index[] Points;

          public enum GXPrimitive {
            GX_QUADS = 128,         // 0x00000080
            GX_TRIANGLES = 144,     // 0x00000090
            GX_TRIANGLESTRIP = 152, // 0x00000098
            GX_TRIANGLEFAN = 160,   // 0x000000A0
            GX_LINES = 168,         // 0x000000A8
            GX_LINESTRIP = 176,     // 0x000000B0
            GX_POINTS = 184,        // 0x000000B8
          }

          public class Index {
            public ushort[] ColorIndex = new ushort[2];
            public ushort[] TexCoordIndex = new ushort[8];
            public ushort MatrixIndex;
            public ushort PosIndex;
            public ushort NormalIndex;
          }
        }
      }
    }
  }
}