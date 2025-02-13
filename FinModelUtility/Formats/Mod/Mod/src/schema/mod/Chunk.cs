using System;
using System.Collections.Generic;

using schema.binary;
using schema.binary.attributes;

namespace mod.schema.mod {
  public enum ChunkId {
    HEADER = 0x00,
    VERTICES = 0x10,
    VERTEX_NORMALS = 0x11,
    VERTEX_NBTS = 0x12,
    VERTEX_COLOURS = 0x13,

    TEX_COORD_0 = 0x18,
    TEX_COORD_1 = 0x19,
    TEX_COORD_2 = 0x1A,
    TEX_COORD_3 = 0x1B,
    TEX_COORD_4 = 0x1C,
    TEX_COORD_5 = 0x1D,
    TEX_COORD_6 = 0x1E,
    TEX_COORD_7 = 0x1F,

    TEXTURES = 0x20,
    TEXTURE_ATTRIBUTES = 0x22,
    MATERIALS = 0x30,

    VERTEX_MATRIX = 0x40,
    MATRIX_ENVELOPE = 0x41,

    MESH = 0x50,
    JOINTS = 0x60,
    JOINT_NAMES = 0x61,

    COLLISION_PRISM = 0x100,
    COLLISION_GRID = 0x110,
    END_OF_FILE = 0xFFFF,
  }

  public static class Chunk {
    public static string GetName(ChunkId id)
      => id switch {
          ChunkId.HEADER => "Header",
          ChunkId.VERTICES => "Vertices",
          ChunkId.VERTEX_NORMALS => "Vertex Normals",
          ChunkId.VERTEX_NBTS => "Vertex Normal/Binormal/Tangent Descriptors",
          ChunkId.VERTEX_COLOURS => "Vertex Colours",

          ChunkId.TEX_COORD_0 => "Texture Coordinate 0",
          ChunkId.TEX_COORD_1 => "Texture Coordinate 1",
          ChunkId.TEX_COORD_2 => "Texture Coordinate 2",
          ChunkId.TEX_COORD_3 => "Texture Coordinate 3",
          ChunkId.TEX_COORD_4 => "Texture Coordinate 4",
          ChunkId.TEX_COORD_5 => "Texture Coordinate 5",
          ChunkId.TEX_COORD_6 => "Texture Coordinate 6",
          ChunkId.TEX_COORD_7 => "Texture Coordinate 7",

          ChunkId.TEXTURES           => "Textures",
          ChunkId.TEXTURE_ATTRIBUTES => "Texture Attributes",
          ChunkId.MATERIALS          => "Materials",

          ChunkId.VERTEX_MATRIX   => "Vertex Matrix",
          ChunkId.MATRIX_ENVELOPE => "Matrix Envelope",

          ChunkId.MESH        => "Mesh",
          ChunkId.JOINTS      => "Joints",
          ChunkId.JOINT_NAMES => "Joint Names",

          ChunkId.COLLISION_PRISM => "Collision Prism",
          ChunkId.COLLISION_GRID  => "Collision Grid",
          ChunkId.END_OF_FILE     => "End Of File",
          _                       => throw new ArgumentOutOfRangeException(nameof(id), id, null)
      };
  }

  [BinarySchema]
  public partial class ChunkData : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.UINT32)]
    public ChunkId Id { get; private set; }

    [SequenceLengthSource(SchemaIntegerType.INT32)]
    public byte[] Data { get; private set; }
  }

  public class ModSectionData : IBinaryConvertible {
    public List<ChunkData> Chunks { get; } = [];
    public List<byte> EofBytes { get; } = [];

    public void Read(IBinaryReader br) {
      var opcodes = new HashSet<uint>();

      while (!opcodes.Contains(0xFFFF)) {
        var chunkData = new ChunkData();
        chunkData.Read(br);

        this.Chunks.Add(chunkData);

        opcodes.Add((uint) chunkData.Id);
      }

      while (!br.Eof) {
        this.EofBytes.Add(br.ReadByte());
      }
    }

    public void Write(IBinaryWriter bw) {
      throw new NotImplementedException();
    }
  }
}