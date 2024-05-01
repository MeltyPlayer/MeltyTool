using fin.math;
using fin.schema;
using fin.schema.matrix;
using fin.schema.vector;

using schema.binary;

namespace visceral.schema.geo {
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

      this.ModelName = br.SubreadAt(br.ReadUInt32(), ser => ser.ReadStringNT());

      br.Position += 0x10;
      var meshCount = br.ReadUInt32();
      var boneCount = br.ReadUInt32();

      br.Position += 0xc;

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
      br.Position += 0x10;
      var uvBufferLength = br.ReadUInt32();
      var totalUvBufferCount = br.ReadUInt32();
      var uvSize = br.ReadUInt16();
      br.Position += 2;
      var uvBufferOffset = br.ReadUInt32();


      br.Position = boneDataOffset;
      var bones = new List<Bone>();
      for (var i = 0; i < boneCount; ++i) {
        var boneName = br.SubreadAt(br.ReadUInt32(), ser => ser.ReadStringNT());
        br.Position += 8;
        var someId = br.ReadUInt32();

        var matrix = br.SubreadAt(boneOffset + 16 * (someId - 1),
                                  ser => ser.ReadNew<Matrix4x4f>());

        bones.Add(new Bone { Name = boneName, Matrix = matrix, Id = someId, });
      }

      this.Bones = bones;


      var meshes = new List<Mesh>();
      for (var i = 0; i < meshCount; i++) {
        br.Position = tableOffset + 0xA0 * i;

        var meshName = br.SubreadAt(br.ReadUInt32(), ser => ser.ReadStringNT());

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


        br.Position = polyInfoOffset;
        var faceCount = br.ReadUInt32();
        br.Position += 4;
        var baseVertexIndex = br.ReadUInt16();
        var vertexCount = br.ReadUInt16();


        br.Position = vertOffset;
        var vertices = new List<Vertex>();
        for (var v = 0; v < vertexCount; v++) {
          var position = br.ReadNew<Vector3f>();

          var normal = this.Read32BitNormal_(br);
          var tangent = this.Read32BitTangent_(br);

          var boneIds = br.ReadBytes(4)
                          .Select(id => br.SubreadAt(
                                      boneIdMappingOffset + 2 * id,
                                      ser => ser.ReadByte()))
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

        br.Position = uvBufferOffset + baseVertexIndex * uvSize;
        for (var u = 0; u < vertexCount; ++u) {
          vertices[u].Uv = br.ReadNew<Vector2f>();
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

    private Vector3f Read32BitNormal_(IBinaryReader br) {
      var vec = new Vector3f();

      var bitsPerAxis = 10;
      var divisor = 512f;

      var value = br.ReadUInt32();
      for (var i = 0; i < 3; ++i) {
        var axisValue = value.ExtractFromRight(bitsPerAxis * i, bitsPerAxis);
        var signedAxisValue = SignValue_(axisValue, bitsPerAxis);
        vec[i] = signedAxisValue / divisor;
      }

      return vec;
    }

    private Vector4f Read32BitTangent_(IBinaryReader br) {
      var vec = new Vector4f();

      var bitsPerAxis = 8;
      var divisor = 127f;

      var value = br.ReadUInt32();
      for (var i = 0; i < 4; ++i) {
        var axisValue = value.ExtractFromRight(bitsPerAxis * i, bitsPerAxis);
        var signedAxisValue = SignValue_(axisValue, bitsPerAxis);
        vec[i] = signedAxisValue / divisor;
      }

      return vec;
    }

    private int SignValue_(uint x, int bitsPerAxis) {
      var isSigned = x.GetBit(bitsPerAxis - 1);
      var signedX = (int) x;

      if (isSigned) {
        var mask = BitLogic.GetMask(bitsPerAxis - 1);
        signedX = (int) (signedX ^ mask);
        signedX++; // Because of 2's complement
        signedX *= -1;
      }

      return signedX;
    }

    public class Bone {
      public required string Name { get; init; }
      public required Matrix4x4f Matrix { get; init; }
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
      public required Vector3f Position { get; init; }
      public required Vector3f Normal { get; init; }
      public required Vector4f Tangent { get; init; }
      public Vector2f Uv { get; set; }
      public required IReadOnlyList<byte> Bones { get; init; }
      public required IReadOnlyList<float> Weights { get; init; }
    }

    public class Face {
      public required IReadOnlyList<ushort> Indices { get; init; }
    }
  }
}