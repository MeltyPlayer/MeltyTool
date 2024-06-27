using schema.binary;

namespace jsystem.schema.j3dgraph.bmd.inf1;

public enum Inf1EntryType : ushort {
  TERMINATOR = 0,
  HIERARCHY_DOWN = 1,
  HIERARCHY_UP = 2,
  JOINT = 16,
  MATERIAL = 17,
  SHAPE = 18,
}

[BinarySchema]
public partial class Inf1Entry : IBinaryConvertible {
  public Inf1EntryType Type { get; set; }
  public ushort Index { get; set; }

  public override string ToString()
    => $"{this.Type} {this.Index}";
}