using fin.color;

using schema.binary;
using schema.binary.attributes;

namespace fin.schema.color;

[BinarySchema]
public partial struct Rgba32(byte r, byte g, byte b, byte a)
    : IColor, IBinaryConvertible {
  public static readonly Rgba32 WHITE = new(255, 255, 255);

  public Rgba32(byte r, byte g, byte b) : this(r, g, b, 255) { }

  public byte Rb { get; private set; } = r;
  public byte Gb { get; private set; } = g;
  public byte Bb { get; private set; } = b;
  public byte Ab { get; private set; } = a;

  [Skip]
  public float Rf => this.Rb / 255f;

  [Skip]
  public float Gf => this.Gb / 255f;

  [Skip]
  public float Bf => this.Bb / 255f;

  [Skip]
  public float Af => this.Ab / 255f;

  public override string ToString()
    => $"rgba({this.Rf}, {this.Gf}, {this.Bf}, {this.Af})";
}