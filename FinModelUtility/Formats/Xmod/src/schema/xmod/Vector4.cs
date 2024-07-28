using fin.model;
using fin.util.asserts;

using schema.text;
using schema.text.reader;

namespace xmod.schema.xmod;

public class Vector4 : ITextDeserializable, IVector4 {
  public Vector4() { }

  public Vector4(float x, float y, float z, float w) {
    this.X = x;
    this.Y = y;
    this.Z = z;
    this.W = w;
    }

  public float X { get; set; }
  public float Y { get; set; }
  public float Z { get; set; }
  public float W { get; set; }

  public void Read(ITextReader tr) {
      var values = tr.ReadSingles(TextReaderConstants.WHITESPACE_STRINGS,
                                  TextReaderConstants.NEWLINE_STRINGS);
      Asserts.Equal(4, values.Length);
      this.X = values[0];
      this.Y = values[1];
      this.Z = values[2];
      this.W = values[2];
    }
}