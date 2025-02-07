using System.Numerics;

using fin.schema;

using schema.text;
using schema.text.reader;

namespace gm.schema.mod {
  public struct ModVertex : ITextDeserializable {
    public int Something { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Normal { get; private set; }
    public Vector2 Uv { get; private set; }

    public void Read(ITextReader tr) {
      this.Something = tr.ReadInt32();
      var singles = tr.ReadSingles(TextReaderConstantsExtra.WHITESPACE_STRINGS,
                                   TextReaderConstants.NEWLINE_STRINGS);

      var i = 0;
      this.Position = new Vector3(singles[i++], singles[i++], singles[i++]);
      this.Normal = new Vector3(singles[i++], singles[i++], singles[i++]);
      this.Uv = new Vector2(singles[i++], singles[i++]);
    }
  }
}