using fin.color;

using schema.binary;
using schema.binary.attributes;

namespace fin.schema.color;

[BinarySchema]
public partial struct Rgba32 : IColor, IBinaryConvertible {
  public byte Rb { get; private set; }
  public byte Gb { get; private set; }
  public byte Bb { get; private set; }
  public byte Ab { get; private set; }

  [Skip]
  public float Rf => this.Rb / 255f;

  [Skip]
  public float Gf => this.Gb / 255f;

  [Skip]
  public float Bf => this.Bb / 255f;

  [Skip]
  public float Af => this.Ab / 255f;

  public override string ToString() => $"rgba({Rf}, {Gf}, {Bf}, {Af})";
}