using fin.schema;

using schema.text;
using schema.text.reader;


namespace xmod.schema.xmod;

public class Material : ITextDeserializable {
  public string Name { get; set; }

  public Vector3 AmbientColor { get; set; }
  public Vector3 DiffuseColor { get; set; }
  public Vector3 SpecularColor { get; set; }

  public int NumPackets { get; set; }
  public IReadOnlyList<TextureId> TextureIds { get; set; }

  public float Shininess { get; set; }

  public void Read(ITextReader tr) {
    tr.AssertString("mtl ");

    this.Name = tr.ReadUpToAndPastTerminator(TextReaderUtils.OPEN_BRACE);

    this.NumPackets = TextReaderUtils.ReadKeyValueNumber<int>(tr, "packets");
    TextReaderUtils.ReadKeyValueNumber<int>(tr, "primitives");
    var numTextures = TextReaderUtils.ReadKeyValueNumber<int>(tr, "textures");
    TextReaderUtils.ReadKeyValue(tr, "illum");

    this.AmbientColor
        = TextReaderUtils.ReadKeyValueInstance<Vector3>(tr, "ambient");
    this.DiffuseColor
        = TextReaderUtils.ReadKeyValueInstance<Vector3>(tr, "diffuse");
    this.SpecularColor
        = TextReaderUtils.ReadKeyValueInstance<Vector3>(tr, "specular");

    this.TextureIds =
        TextReaderUtils.ReadKeyValueInstances<TextureId>(
            tr,
            "texture",
            numTextures);

    // TODO: Attributes
    var attributeCount
        = TextReaderUtils.ReadKeyValueNumber<int>(tr, "attributes");
    for (var i = 0; i < attributeCount; ++i) {
      var type = tr.ReadWord();
      var (key, value) = TextReaderUtils.ReadKeyValue(tr);
      switch (key) {
        case "shininess": {
          this.Shininess = float.Parse(value);
          break;
        }
      }
    }

    tr.ReadUpToAndPastTerminator(TextReaderUtils.CLOSING_BRACE);
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
  }
}