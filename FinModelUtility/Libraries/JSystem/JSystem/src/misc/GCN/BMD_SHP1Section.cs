using System;
using System.Collections.Generic;
using System.Linq;

using fin.schema;

using gx;
using gx.displayList;

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

        var batchAttributes = br.ReadNew<BatchAttributes>();

        this.Packets = new Packet[(int) this.NrPacket];
        this.PacketLocations = new PacketLocation[(int) this.NrPacket];
        for (int index = 0; index < (int) this.NrPacket; ++index) {
          br.Position = baseoffset +
                        (long) Parent.PacketLocationsOffset +
                        (long) (((int) this.FirstPacketLocation + index) * 8);
          var packetLocation = new PacketLocation();
          packetLocation.Read(br);
          this.PacketLocations[index] = packetLocation;

          br.Position = baseoffset +
                        (long) Parent.DataOffset +
                        (long) this.PacketLocations[index].Offset;
          this.Packets[index] = new Packet(br,
                                           (int) this.PacketLocations[index]
                                               .Size,
                                           batchAttributes);
          br.Position = baseoffset +
                        (long) Parent.MatrixDataOffset +
                        (long) (((int) this.FirstMatrixData + index) * 8);
          this.Packets[index].MatrixData = br.ReadNew<MatrixData>();
          br.Position = baseoffset +
                        (long) Parent.MatrixTableOffset +
                        (long) (2U *
                                this.Packets[index].MatrixData
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
            IVertexDescriptor vertexDescriptor) {
          List<Primitive> primitiveList = [];
          var gxDisplayListReader = new GxDisplayListReader();
          br.Subread(
              Length,
              () => {
                while (!br.Eof) {
                  var gxPrimitive
                      = gxDisplayListReader.Read(br, vertexDescriptor);
                  if (gxPrimitive == null) {
                    continue;
                  }

                  var primitive = new Primitive();
                  primitive.Type = gxPrimitive.PrimitiveType;
                  primitive.Points
                      = gxPrimitive
                        .Vertices
                        .Select(v => {
                          var index = new Primitive.Index();

                          index.PosIndex = v.PositionIndex;
                          index.MatrixIndex = v.JointIndex ?? 0;
                          index.NormalIndex = v.NormalIndex;
                          index.ColorIndices = v.ColorIndices;
                          index.TexCoordIndices = v.TexCoordIndices;

                          return index;
                        })
                        .ToArray();

                  primitiveList.Add(primitive);
                }
              });

          this.Primitives = primitiveList.ToArray();
        }

        public class Primitive {
          public GxPrimitiveType Type;
          public Index[] Points;

          public class Index {
            public ushort?[] ColorIndices;
            public ushort?[] TexCoordIndices;
            public ushort MatrixIndex;
            public ushort PosIndex;
            public ushort? NormalIndex;
          }
        }
      }
    }
  }
}