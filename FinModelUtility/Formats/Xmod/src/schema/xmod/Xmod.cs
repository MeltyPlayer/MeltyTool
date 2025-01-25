using schema.text;
using schema.text.reader;

namespace xmod.schema.xmod;

public class Xmod : ITextDeserializable {
  public IReadOnlyList<Vector3> Positions { get; set; }
  public IReadOnlyList<Vector3> Normals { get; set; }
  public IReadOnlyList<Vector4> Colors { get; set; }
  public IReadOnlyList<Vector2> Uv1s { get; set; }
  public IReadOnlyList<Material> Materials { get; set; }
  public IReadOnlyList<Packet> Packets { get; set; }

  public void Read(ITextReader tr) {
      var version = TextReaderUtils.ReadKeyValue(tr, "version");

      var numVertices = TextReaderUtils.ReadKeyValueNumber<int>(tr, "verts");
      var numNormals = TextReaderUtils.ReadKeyValueNumber<int>(tr, "normals");
      var numColors = TextReaderUtils.ReadKeyValueNumber<int>(tr, "colors");
      var numUv1s = TextReaderUtils.ReadKeyValueNumber<int>(tr, "tex1s");
      var numUv2s = TextReaderUtils.ReadKeyValueNumber<int>(tr, "tex2s");
      var numTangents =
          TextReaderUtils.ReadKeyValueNumber<int>(tr, "tangents");
      var numMaterials =
          TextReaderUtils.ReadKeyValueNumber<int>(tr, "materials");
      var numAdjuncts =
          TextReaderUtils.ReadKeyValueNumber<int>(tr, "adjuncts");
      var numPrimitives =
          TextReaderUtils.ReadKeyValueNumber<int>(tr, "primitives");
      var numMatrices =
          TextReaderUtils.ReadKeyValueNumber<int>(tr, "matrices");
      var numReskins = TextReaderUtils.ReadKeyValueNumber<int>(tr, "reskins");
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

      this.Positions = TextReaderUtils.ReadInstances<Vector3>(tr, "v", numVertices);
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

      this.Normals = TextReaderUtils.ReadInstances<Vector3>(tr, "n", numNormals);
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

      this.Colors = TextReaderUtils.ReadInstances<Vector4>(tr, "c", numColors);
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

      this.Uv1s =
          TextReaderUtils.ReadInstances<Vector2>(tr, "t1", numUv1s);
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      var uv2s =
          TextReaderUtils.ReadInstances<Vector2>(tr, "t2", numUv2s);
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

      this.Materials = tr.ReadNews<Material>(numMaterials);

      var numPackets = this.Materials.Select(material => material.NumPackets).Sum();
      this.Packets = tr.ReadNews<Packet>(numPackets);
    }
}