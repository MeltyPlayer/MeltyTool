using schema.text;
using schema.text.reader;


namespace xmod.schema.xmod;

public class Material : ITextDeserializable {
  public string Name { get; set; }

  public int NumPackets { get; set; }
  public IReadOnlyList<TextureId> TextureIds { get; set; }

  public void Read(ITextReader tr) {
    tr.AssertString("mtl ");

    this.Name = tr.ReadUpToAndPastTerminator(TextReaderUtils.OPEN_BRACE);

    this.NumPackets = TextReaderUtils.ReadKeyValueNumber<int>(tr, "packets");
    TextReaderUtils.ReadKeyValueNumber<int>(tr, "primitives");
    var numTextures = TextReaderUtils.ReadKeyValueNumber<int>(tr, "textures");
    TextReaderUtils.ReadKeyValue(tr, "illum");
    TextReaderUtils.ReadKeyValueInstance<Vector3>(tr, "ambient");
    TextReaderUtils.ReadKeyValueInstance<Vector3>(tr, "diffuse");
    TextReaderUtils.ReadKeyValueInstance<Vector3>(tr, "specular");

    this.TextureIds =
        TextReaderUtils.ReadKeyValueInstances<TextureId>(
            tr,
            "texture",
            numTextures);

    // TODO: Attributes
    //TextReaderUtils.ReadKeyValueNumber<int>(tr, "attributes");

    tr.ReadUpToAndPastTerminator(TextReaderUtils.CLOSING_BRACE);
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
  }
}