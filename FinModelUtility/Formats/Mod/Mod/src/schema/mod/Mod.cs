using System;
using System.Collections.Generic;
using System.Text;

using fin.schema.color;
using fin.schema.vector;
using fin.util.asserts;

using mod.schema.mod.collision;

using schema.binary;

namespace mod.schema.mod {
  [BinarySchema]
  public partial class DateTime : IBinaryConvertible {
    public ushort year { get; set; } = 2021;
    public byte month { get; set; } = 9;
    public byte day { get; set; } = 18;
  }

  [BinarySchema]
  public partial class ModHeader : IBinaryConvertible {
    public readonly DateTime dateTime = new();

    public uint flags { get; set; }
  }

  public enum ModFlags {
    UseNBT = 0x01
  }

  public class Mod : IBinaryConvertible {
    public readonly ModHeader header = new();
    public readonly List<Vector3f> vertices = [];
    public readonly List<Vector3f> vnormals = [];
    public readonly List<Nbt> vertexnbt = [];
    public readonly List<Rgba32> vcolours = [];
    public readonly List<Vector2f>[] texcoords = new List<Vector2f>[8];
    public readonly List<Texture> textures = [];
    public readonly List<TextureAttributes> texattrs = [];
    public readonly MaterialContainer materials = new();
    public readonly List<VtxMatrix> vtxMatrix = [];
    public readonly List<Envelope> envelopes = [];
    public readonly List<Mesh> meshes = [];
    public readonly List<Joint> joints = [];
    public readonly List<string> jointNames = [];
    public readonly CollTriInfo colltris = new();
    public readonly CollGrid collgrid = new();
    public readonly List<byte> eofBytes = [];
    public bool hasNormals = false;

    public Mod() {}
    public Mod(IBinaryReader reader) => this.Read(reader);

    public void Read(IBinaryReader br) {
      this.hasNormals = false;

      for (var i = 0; i < 8; ++i) {
        this.texcoords[i] = [];
      }

      bool stopRead = false;
      while (!stopRead) {
        var position = br.Position;

        var chunkId = (ChunkId) br.ReadUInt32();
        var chunkName = Chunk.GetName(chunkId);

        var length = br.ReadUInt32();

        if ((position & 0x1F) != 0) {
          Asserts.Fail("Error in chunk " +
                       chunkId +
                       ", offset " +
                       position +
                       ", chunk start isn't aligned to 0x20, this means an improper read occured.");
          return;
        }

        /*std::cout <<
            "Reading 0x" <<
            std::hex <<
            opcode <<
            std::dec <<
            ", " <<
            (ocString.has_value() ? ocString.value() : "Unknown chunk") <<
            std::endl;*/

        var beforePosition = br.Position;

        switch (chunkId) {
          case ChunkId.HEADER:
            br.Align(0x20);
            this.header.Read(br);
            br.Align(0x20);
            break;
          case ChunkId.VERTICES:
            Mod.ReadGenericChunk_(br, this.vertices);
            break;
          case ChunkId.VERTEX_NORMALS:
            this.hasNormals = true;
            Mod.ReadGenericChunk_(br, this.vnormals);
            break;
          case ChunkId.VERTEX_NBTS:
            Mod.ReadGenericChunk_(br, this.vertexnbt);
            break;
          case ChunkId.VERTEX_COLOURS:
            Mod.ReadGenericChunk_(br, this.vcolours);
            break;
          case >= ChunkId.TEX_COORD_0 and <= ChunkId.TEX_COORD_7:
            Mod.ReadGenericChunk_(br, this.texcoords[(uint) chunkId - 0x18]);
            break;
          case ChunkId.TEXTURES:
            Mod.ReadGenericChunk_(br, this.textures);
            for (var i = 0; i < this.textures.Count; ++i) {
              this.textures[i].index = i;
            }
            break;
          case ChunkId.TEXTURE_ATTRIBUTES:
            Mod.ReadGenericChunk_(br, this.texattrs);
            break;
          case ChunkId.MATERIALS:
            var numMaterials = br.ReadUInt32();
            var numTexEnvironments = br.ReadUInt32();

            br.Align(0x20);
            this.materials.texEnvironments.Clear();
            for (var i = 0; i < numTexEnvironments; ++i) {
              var texEnvironment = new TEVInfo();
              texEnvironment.Read(br);
              this.materials.texEnvironments.Add(texEnvironment);
            }

            this.materials.materials.Clear();
            for (var i = 0; i < numMaterials; ++i) {
              var mat = new Material();
              mat.Read(br);
              this.materials.materials.Add(mat);
            }
            br.Align(0x20);

            var readLength = br.Position - beforePosition;

            ;
            break;
          case ChunkId.VERTEX_MATRIX:
            Mod.ReadGenericChunk_(br, this.vtxMatrix);
            break;
          case ChunkId.MATRIX_ENVELOPE:
            Mod.ReadGenericChunk_(br, this.envelopes);
            break;
          case ChunkId.MESH:
            Mod.ReadGenericChunk_(br, this.meshes);
            break;
          case ChunkId.JOINTS:
            Mod.ReadGenericChunk_(br, this.joints);
            break;
          case ChunkId.JOINT_NAMES:
            var numJointNames = br.ReadUInt32();
            this.jointNames.Clear();
            br.Align(0x20);
            for (var i = 0; i < numJointNames; ++i) {
              var jointNameLength = br.ReadUInt32();

              var jointNameBuilder = new StringBuilder((int) jointNameLength);
              for (var c = 0; c < jointNameLength; ++c) {
                jointNameBuilder.Append(br.ReadChar());
              }
            }
            br.Align(0x20);
            break;
          case ChunkId.COLLISION_PRISM:
            this.colltris.Read(br);
            break;
          case ChunkId.COLLISION_GRID:
            this.collgrid.Read(br);
            break;
          case ChunkId.END_OF_FILE:
            br.Position += length;

            while (!br.Eof) {
              this.eofBytes.Add(br.ReadByte());
            }

            stopRead = true;
            break;
          default:
            br.Position += length;
            break;
        }

        var afterPosition = br.Position;

        /*Asserts.Equal(beforePosition + length,
                      afterPosition,
                      $"Read incorrect number of bytes for opcode: {opcodeName}");*/
      }

      ;
    }

    private static void ReadGenericChunk_<T>(
        IBinaryReader br,
        List<T> vector) where T : IBinaryDeserializable, new() {
      var num = br.ReadUInt32();
      vector.Clear();

      br.Align(0x20);
      for (var i = 0; i < num; ++i) {
        vector.Add(Mod.ReadGeneric_<T>(br));
      }
      br.Align(0x20);
    }

    private static T ReadGeneric_<T>(IBinaryReader br)
        where T : IBinaryDeserializable, new() {
      var instance = new T();
      instance.Read(br);
      return instance;
    }

    public void Write(IBinaryWriter bw) {
      throw new NotImplementedException();
    }
  }
}


/*static inline void writeGenericChunk(util::fstream_writer
&writer, auto & vector,
u32 chunkIdentifier) {
  std::cout <<
      "Writing 0x" <<
      std::hex <<
      chunkIdentifier <<
      std::dec <<
      ", " <<
      MOD::getChunkName(chunkIdentifier).value() <<
      std::endl;

  u32 subchunkPos = startChunk(writer, chunkIdentifier);
  writer.writeU32(vector.size());

  writer.align(0x20);
  for (auto & contents : vector)
  {
    contents.write(writer);
  }
  finishChunk(writer, subchunkPos);
}
}*/

// NOTE: the control flow and layout of this function is a replica of a
// decompiled version of the DMD->MOD process, found in plugTexConv
/*void MOD::write(util::fstream_writer & writer) {
  // Write header
  u32 headerPos = startChunk(writer, 0);
  writer.align(0x20);
  writer.writeU16(m_header.m_dateTime.m_year);
  writer.writeU8(m_header.m_dateTime.m_month);
  writer.writeU8(m_header.m_dateTime.m_day);
  writer.writeU32(m_header.m_flags);
  finishChunk(writer, headerPos);

  if (m_vertices.size()) {
    writeGenericChunk(writer, m_vertices, 0x10);
  }

  if (m_vcolours.size()) {
    writeGenericChunk(writer, m_vcolours, 0x13);
  }

  if (m_vnormals.size()) {
    writeGenericChunk(writer, m_vnormals, 0x11);
  }

  if (m_header.m_flags & static_cast<u32>(MODFlags::UseNBT) &&
      m_vertexnbt.size()) {
    writeGenericChunk(writer, m_vnormals, 0x12);
  }

  for (std::size_t i = 0; i < m_texcoords.size(); i++) {
    if (m_texcoords[i].size()) {
      writeGenericChunk(writer, m_texcoords[i], i + 0x18);
    }
  }

  if (m_textures.size()) {
    writeGenericChunk(writer, m_textures, 0x20);
  }

  if (m_texattrs.size()) {
    writeGenericChunk(writer, m_texattrs, 0x22);
  }

  if (m_materials.m_materials.size()) {
    std::cout <<
        "Writing 0x30, " <<
        MOD::getChunkName(0x30).value() <<
        std::endl;

    const u32 start = startChunk(writer, 0x30);
    writer.writeU32(m_materials.m_materials.size());
    writer.writeU32(m_materials.m_texEnvironments.size());
    writer.align(0x20);

    for (mat::TEVInfo & tevInfo : m_materials.m_texEnvironments)
    {
      tevInfo.write(writer);
    }

    for (mat::Material & material : m_materials.m_materials)
    {
      material.write(writer);
    }
    finishChunk(writer, start);
  }

  if (m_envelopes.size()) {
    writeGenericChunk(writer, m_envelopes, 0x41);
  }

  if (m_vtxMatrix.size()) {
    writeGenericChunk(writer, m_vtxMatrix, 0x40);
  }

  if (m_meshes.size()) {
    writeGenericChunk(writer, m_meshes, 0x50);
  }

  if (m_joints.size()) {
    writeGenericChunk(writer, m_joints, 0x60);

    if (m_jointNames.size()) {
      std::cout <<
          "Writing 0x61, " <<
          MOD::getChunkName(0x61).value() <<
          std::endl;

      const u32 start = startChunk(writer, 0x61);
      writer.writeU32(m_jointNames.size());
      writer.align(0x20);
      for (std:: string & name :
      m_jointNames) {
        writer.writeU32(name.size());
        for (std::size_t i = 0; i < name.size(); i++) {
          writer.writeU8(name[i]);
        }
      }
      finishChunk(writer, start);
    }
  }

  if (m_colltris.m_collinfo.size()) {
    std::cout <<
        "Writing 0x100, " <<
        MOD::getChunkName(0x100).value() <<
        std::endl;
    m_colltris.write(writer);

    const u32 start = startChunk(writer, 0x110);
    writer.align(0x20);
    m_collgrid.m_boundsMin.write(writer);
    m_collgrid.m_boundsMax.write(writer);
    writer.writeF32(m_collgrid.m_unknown1);
    writer.writeU32(m_collgrid.m_gridX);
    writer.writeU32(m_collgrid.m_gridY);
    writer.writeU32(m_collgrid.m_groups.size());
    for (CollGroup & group : m_collgrid.m_groups)
    {
      group.write(writer);
    }
    for (s32 & i : m_collgrid.m_unknown2)
    {
      writer.writeS32(i);
    }
    writer.align(0x20);
    finishChunk(writer, start);
  }

  // Finalise writing with 0xFFFF chunk and append any INI file
  finishChunk(writer, startChunk(writer, 0xFFFF));
  if (m_eofBytes.size()) {
    std::cout <<
        "Writing 0xffff, " <<
        MOD::getChunkName(0xffff).value() <<
        std::endl;

    writer.write(reinterpret_cast<char*>(m_eofBytes.data()),
                 m_eofBytes.size());
  }
}*/