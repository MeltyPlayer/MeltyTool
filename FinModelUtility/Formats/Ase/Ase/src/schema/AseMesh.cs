using System.Numerics;
using System.Text;

using schema.binary;
using schema.binary.attributes;

namespace rollingMadness.schema;

[BinarySchema]
public partial class AseMesh : IBinaryConvertible {
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public AseString[] ImageNames0 { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public AseString[] ImageNames1 { get; set; }

  public uint Unk0 { get; set; }
  public uint Unk1 { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public Vertex[] Vertices { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public UvData[] UvDatas { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public Triangle[] Triangles { get; set; }
}

[BinarySchema]
public partial class AseString : IBinaryConvertible {
  public string Value { get; set; }

  public void Read(IBinaryReader br) {
    br.AssertChar('"');

    var sb = new StringBuilder();
    char c;
    while (!br.Eof) {
      c = br.ReadChar();
      if (c == '"') {
        break;
      }

      sb.Append(c);
    }

    this.Value = sb.ToString();
  }

  public void Write(IBinaryWriter bw) {
    bw.WriteChar('"');
    bw.WriteString(this.Value);
    bw.WriteChar('"');
  }
}

[BinarySchema]
public partial class Vertex : IBinaryConvertible {
  public Vector3 Position { get; set; }
  public Vector3 Normal { get; set; }
}

[BinarySchema]
public partial class UvData : IBinaryConvertible {
  public Vector2 Uv { get; set; }
  public float Unk0 { get; set; }
  public float Unk1 { get; set; }
  public Vector2 LightmapUv { get; set; }
}

[BinarySchema]
public partial class Triangle : IBinaryConvertible {
  public uint Vertex1 { get; set; }
  public uint Vertex2 { get; set; }
  public uint Vertex3 { get; set; }
  public uint MaterialIndex { get; set; }
  public int Unk1 { get; set; }
  public int Unk2 { get; set; }
  public int Unk3 { get; set; }
}