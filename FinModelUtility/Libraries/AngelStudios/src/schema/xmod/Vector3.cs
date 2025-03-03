using fin.math.xyz;
using fin.schema;
using fin.util.asserts;

using schema.text;
using schema.text.reader;


namespace xmod.schema.xmod;

public class Vector3 : ITextDeserializable, IXyz {
  public Vector3() { }

  public Vector3(float x, float y, float z) {
    this.X = x;
    this.Y = y;
    this.Z = z;
  }

  public float X { get; set; }
  public float Y { get; set; }
  public float Z { get; set; }

  public void Read(ITextReader tr) {
    var values = tr.ReadSingles(TextReaderConstants.WHITESPACE_STRINGS,
                                TextReaderConstants.NEWLINE_STRINGS);
    Asserts.Equal(3, values.Length);
    this.X = values[0];
    this.Y = values[1];
    this.Z = values[2];
  }
}