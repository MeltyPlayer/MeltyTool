using fin.math;
using fin.schema.matrix;
using fin.schema.vector;

using schema.binary;


namespace visceral.schema.geo {
  public class Geo : IBinaryDeserializable {
    public string ModelName { get; set; }

    public IReadOnlyList<Bone> Bones { get; set; }
    public IReadOnlyList<Mesh> Meshes { get; set; }

    public void Read(IEndianBinaryReader er) {
      er.AssertString("MGAE");

      er.Position += 8;
      er.AssertUInt32((uint) er.Length);

      er.Position += 0x10;

      this.ModelName = er.ReadStringNTAtOffset(er.ReadUInt32());

      er.Position += 0x10;
      var meshCount = er.ReadUInt32();
      var boneCount = er.ReadUInt32();

      er.Position += 0xc;

      var refCount = er.ReadUInt32();
      var refTableOffset = er.ReadUInt32();

      var tableOffset = er.ReadUInt32();
      var unkOffset = er.ReadUInt32();

      er.Position += 8;

      var boneDataOffset = er.ReadUInt32();
      var boneOffset = er.ReadUInt32();
      var uvBufferInfoOffset = er.ReadUInt32();
      var faceBufferInfoOffset = er.ReadUInt32();


      er.Position = uvBufferInfoOffset;
      er.Position += 0x10;
      var uvBufferLength = er.ReadUInt32();
      var totalUvBufferCount = er.ReadUInt32();
      var uvSize = er.ReadUInt16();
      er.Position += 2;
      var uvBufferOffset = er.ReadUInt32();


      er.Position = boneDataOffset;
      var bones = new List<Bone>();
      for (var i = 0; i < boneCount; ++i) {
        var boneName = er.ReadStringNTAtOffset(er.ReadUInt32());
        er.Position += 8;
        var someId = er.ReadUInt32();

        var matrix =
            er.ReadNewAtOffset<Matrix4x4f>(boneOffset + 16 * (someId - 1));

        bones.Add(new Bone { Name = boneName, Matrix = matrix, Id = someId, });
      }

      this.Bones = bones;


      var meshes = new List<Mesh>();
      for (var i = 0; i < meshCount; i++) {
        er.Position = tableOffset + 0xC0 * i;

        var meshName = er.ReadStringNTAtOffset(er.ReadUInt32());

        er.Position += 0x1c;
        er.Position += 0x10;

        var polyInfoOffset = er.ReadUInt32();
        er.Position += 0x4;

        var vertOffset = er.ReadUInt32();
        er.Position += 0x4;

        var faceOffset = er.ReadUInt32();
        er.Position += 0x4;


        er.Position = polyInfoOffset;
        var faceCount = er.ReadUInt32();
        er.Position += 4;
        var baseVertexIndex = er.ReadUInt16();
        var vertexCount = er.ReadUInt16();


        er.Position = vertOffset;
        var vertices = new List<Vertex>();
        for (var v = 0; v < vertexCount; v++) {
          var position = er.ReadNew<Vector3f>();

          var normal = this.Read32BitNormal_(er);
          var tangent = this.Read32BitTangent_(er);

          var boneIds = er.ReadBytes(4);
          var weights = er.ReadUn16s(4);

          vertices.Add(new Vertex {
              Position = position,
              Normal = normal,
              Tangent = tangent,
              Bones = boneIds,
              Weights = weights,
          });
        }

        er.Position = uvBufferOffset + baseVertexIndex * uvSize;
        for (var u = 0; u < vertexCount; ++u) {
          vertices[u].Uv = er.ReadNew<Vector2f>();
        }

        er.Position = faceOffset;
        var faces = new List<Face>();
        for (var f = 0; f < faceCount / 3; ++f) {
          var vertexIndices = er.ReadUInt16s(3);
          faces.Add(new Face { Indices = vertexIndices, });
        }

        meshes.Add(new Mesh {
            Name = meshName,
            BaseVertexIndex = baseVertexIndex,
            Vertices = vertices,
            Faces = faces,
        });
      }

      this.Meshes = meshes;
    }

    private Vector3f Read32BitNormal_(IEndianBinaryReader er) {
      var vec = new Vector3f();

      var bitsPerAxis = 10;
      var divisor = 512f;

      var value = er.ReadUInt32();
      for (var i = 0; i < 3; ++i) {
        var axisValue = value.ExtractFromRight(bitsPerAxis * i, bitsPerAxis);
        var signedAxisValue = SignValue_(axisValue, bitsPerAxis);
        vec[i] = signedAxisValue / divisor;
      }

      return vec;
    }

    private Vector4f Read32BitTangent_(IEndianBinaryReader er) {
      var vec = new Vector4f();

      var bitsPerAxis = 8;
      var divisor = 127f;

      var value = er.ReadUInt32();
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