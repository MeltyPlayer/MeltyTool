using fin.color;

using schema.binary;
using schema.binary.attributes;

namespace fin.schema.color;

[BinarySchema]
public partial struct Rgb24 : IColor, IBinaryConvertible {
  public byte Rb { get; set; }
  public byte Gb { get; set; }
  public byte Bb { get; set; }

  [Skip]
  public byte Ab => 255;

  [Skip]
  public float Rf => this.Rb / 255f;

  [Skip]
  public float Gf => this.Gb / 255f;

  [Skip]
  public float Bf => this.Bb / 255f;

  [Skip]
  public float Af => this.Ab / 255f;

  public override int GetHashCode() => this.ToBgraInt();

  public override bool Equals(object? obj) {
    if (obj is IColor otherGeneric) {
      return this.GetHashCode() == otherGeneric.GetHashCode();
    }

    return false;
  }
}