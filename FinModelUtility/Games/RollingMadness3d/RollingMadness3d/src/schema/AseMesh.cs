using System.Numerics;
using System.Text;

using schema.binary;
using schema.binary.attributes;

namespace rollingMadness.schema;

[BinarySchema]
public sealed partial class AseMesh : IBinaryConvertible {
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public AseString[] ImageNames { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public AseString[] LightmapNames { get; set; }

  /// <summary>
  /// Number of animated poses stored after the base pose. The vertex array is
  /// laid out as (AdditionalFrameCount + 1) equally-sized frames.
  /// </summary>
  public uint AdditionalFrameCount { get; set; }

  /// <summary>Duration of the complete vertex animation, in seconds.</summary>
  public float AnimationDuration { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public Vertex[] Vertices { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public UvData[] UvDatas { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public Triangle[] Triangles { get; set; }
}

[BinarySchema]
public sealed partial class AseString : IBinaryConvertible {
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
public sealed partial class Vertex : IBinaryConvertible {
  public Vector3 Position { get; set; }
  public Vector3 Normal { get; set; }
}

[BinarySchema]
public sealed partial class UvData : IBinaryConvertible {
  public Vector2 Uv0 { get; set; }
  public Vector2 Uv1 { get; set; }
  public Vector2 LightmapUv { get; set; }
}

[BinarySchema]
public sealed partial class Triangle : IBinaryConvertible {
  public uint Vertex1 { get; set; }
  public uint Vertex2 { get; set; }
  public uint Vertex3 { get; set; }
  public int MainTextureIndex { get; set; }
  public int DecalTextureIndex { get; set; }
  public int LightmapIndex { get; set; }
  /// <summary>Engine-specific per-face rendering flags.</summary>
  public int RenderFlags { get; set; }
}
