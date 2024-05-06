using fin.schema;
using fin.schema.vector;

using schema.binary;

namespace grezzo.schema.cmb.qtrs {
  [BinarySchema]
  public partial class BoundingBox : IBinaryConvertible {
    // M-1 checked all files, and Min/Max are the only values to ever change
    [Unknown]
    public uint unk0 { get; private set; }

    [Unknown]
    public uint unk1 { get; private set; }

    public Vector3f min { get; } = new();
    public Vector3f max { get; } = new();

    [Unknown]
    public int unk2 { get; private set; }

    [Unknown]
    public int unk3 { get; private set; }

    [Unknown]
    public uint unk4 { get; private set; }
  }
}