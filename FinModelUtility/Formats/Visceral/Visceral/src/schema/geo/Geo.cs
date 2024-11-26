using System.Collections;
using System.Numerics;

using fin.math;
using fin.schema;
using fin.schema.matrix;

using schema.binary;

using visceral.api;

namespace visceral.schema.geo;

public class Geo : IBinaryDeserializable {
  public string ModelName { get; set; }

  public IReadOnlyList<Bone> Bones { get; set; }
  public IReadOnlyList<Mesh> Meshes { get; set; }

  [Unknown]
  public void Read(IBinaryReader br) {
    br.AssertString("MGAE");

    br.Position += 8;
    br.AssertUInt32((uint) br.Length);

    br.Position += 0x10;

    this.ModelName = br.SubreadStringNTAt(br.ReadUInt32());

    br.Position += 0x10;
    var meshCount = br.ReadUInt32();
    var boneCount = br.ReadUInt32();

    var uvBufferInfoCount = br.ReadUInt16();
    var faceBufferInfoCount = br.ReadUInt16();

    br.Position += 0x8;

    var refCount = br.ReadUInt32();
    var refTableOffset = br.ReadUInt32();

    var tableOffset = br.ReadUInt32();
    var unkOffset = br.ReadUInt32();

    br.Position += 8;

    var boneDataOffset = br.ReadUInt32();
    var boneOffset = br.ReadUInt32();
    var uvBufferInfoOffset = br.ReadUInt32();
    var faceBufferInfoOffset = br.ReadUInt32();


    br.Position = uvBufferInfoOffset;
    var uvBufferOffsets = new uint[uvBufferInfoCount / 2];
    var uvSizes = new ushort[uvBufferInfoCount / 2];
    for (var i = 0; i < uvBufferOffsets.Length; ++i) {
      var unkUvStuff1 = br.ReadBytes(0x10);
      var uvBufferLength = br.ReadUInt32();
      var totalUvBufferCount = br.ReadUInt32();
      uvSizes[i] = br.ReadUInt16();
      var unkUvStuff2 = br.ReadBytes(2);
      uvBufferOffsets[i] = br.ReadUInt32();
    }


    br.Position = boneDataOffset;
    var bones = new List<Bone>();
    for (var i = 0; i < boneCount; ++i) {
      var boneName = br.SubreadStringNTAt(br.ReadUInt32());
      br.Position += 8;
      var someId = br.ReadUInt32();

      var matrix
          = br.SubreadAt(boneOffset + 16 * (someId - 1), br.ReadMatrix4x4);

      bones.Add(new Bone { Name = boneName, Matrix = matrix, Id = someId, });
    }

    this.Bones = bones;


    var meshes = new List<Mesh>();
    for (var i = 0; i < meshCount; i++) {
      br.Position = tableOffset + 0xA0 * i;

      var meshName = br.SubreadStringNTAt(br.ReadUInt32());

      br.Position += 8;

      var mtlbId = br.ReadUInt32();

      br.Position += 0x10;
      br.Position += 0x10;

      var polyInfoOffset = br.ReadUInt32();
      br.Position += 0x4;

      var vertOffset = br.ReadUInt32();
      br.Position += 0x4;

      var faceOffset = br.ReadUInt32();
      br.Position += 0x4;

      var boneIdMappingOffset = br.ReadUInt32();

      br.Position += 0x4;
      br.Position += 0x10;
      br.Position += 0x4;

      var uvIndex = br.ReadUInt32();

      br.Position = polyInfoOffset;
      var faceCount = br.ReadUInt32();
      br.Position += 4;
      var baseVertexIndex = br.ReadUInt16();
      var vertexCount = br.ReadUInt16();

      var allBoneIds
          = br.SubreadAt(boneIdMappingOffset,
                         () => br.ReadBytes(2 * boneCount));

      br.Position = vertOffset;
      var vertices = new List<Vertex>();
      for (var v = 0; v < vertexCount; v++) {
        var position = new Vector3 {
            X = br.ReadSingle(),
            Y = br.ReadSingle(),
            Z = br.ReadSingle()
        };

        var normalInt = br.ReadUInt32();
        var tangentInt = br.ReadUInt32();

        var normal = PackedVectorUtil.ExtractNormalFromUInt32(normalInt);
        var tangent
            = PackedVectorUtil.ExtractTangentFromUInt32(tangentInt);

        var boneIds = br.ReadBytes(4)
                        .Select(id => allBoneIds[2 * id])
                        .ToArray();

        var weights = br.ReadUn16s(4);

        vertices.Add(new Vertex {
            Position = position,
            Normal = normal,
            Tangent = tangent,
            Bones = boneIds,
            Weights = weights,
        });
      }

      var uvSize = uvSizes[uvIndex];
      for (var u = 0; u < vertexCount; ++u) {
        br.Position = uvBufferOffsets[uvIndex] + (baseVertexIndex + u) * uvSize;

        var vertex = vertices[u];
        vertex.Uv = new Vector2 {
            X = br.ReadSingle(),
            Y = br.ReadSingle()
        };

        if (uvSize == 20) {
          // TODO: Figure out what this is
          br.Position += 2 * 4;

          vertex.Color = br.ReadInt32();
        }
      }

      br.Position = faceOffset;
      var faces = new List<Face>();
      for (var f = 0; f < faceCount / 3; ++f) {
        var vertexIndices = br.ReadUInt16s(3);
        faces.Add(new Face { Indices = vertexIndices, });
      }

      meshes.Add(new Mesh {
          Name = meshName,
          MtlbId = mtlbId,
          BaseVertexIndex = baseVertexIndex,
          Vertices = vertices,
          Faces = faces,
      });
    }

    this.Meshes = meshes;
  }

  public class Bone {
    public required string Name { get; init; }
    public required Matrix4x4 Matrix { get; init; }
    public required uint Id { get; init; }
  }

  public class Mesh {
    public required string Name { get; init; }
    public required uint MtlbId { get; init; }
    public required ushort BaseVertexIndex { get; init; }
    public required IReadOnlyList<Vertex> Vertices { get; init; }
    public required IReadOnlyList<Face> Faces { get; init; }
  }

  public class Vertex {
    public required Vector3 Position { get; init; }
    public required Vector3 Normal { get; init; }
    public required Vector4 Tangent { get; init; }
    public Vector2 Uv { get; set; }
    public int? Color { get; set; }
    public required IReadOnlyList<byte> Bones { get; init; }
    public required IReadOnlyList<float> Weights { get; init; }
  }

  public class Face {
    public required IReadOnlyList<ushort> Indices { get; init; }
  }
}