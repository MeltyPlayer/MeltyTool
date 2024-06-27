using schema.text;
using schema.text.reader;

namespace xmod.schema.xmod;

public enum PrimitiveType {
  TRIANGLE_STRIP,
  TRIANGLE_STRIP_REVERSED,
  TRIANGLES,
}

public class Primitive : ITextDeserializable {
  public PrimitiveType Type { get; set; }
  public IReadOnlyList<int> VertexIndices { get; set; }

  public void Read(ITextReader tr) {
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_STRINGS);

      this.Type = tr.ReadString(3) switch {
        "stp" => PrimitiveType.TRIANGLE_STRIP,
        "str" => PrimitiveType.TRIANGLE_STRIP_REVERSED,
        "tri" => PrimitiveType.TRIANGLES,
      };

      this.VertexIndices = tr.ReadInt32s(TextReaderConstants.WHITESPACE_STRINGS,
                                         TextReaderConstants.NEWLINE_STRINGS);
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_STRINGS);
    }
}