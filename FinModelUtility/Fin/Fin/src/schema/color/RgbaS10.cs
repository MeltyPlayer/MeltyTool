using System.Drawing;

using fin.color;
using fin.math;

using schema.binary;

namespace fin.schema.color;

[BinarySchema]
public partial class RgbaS10 : IBinaryConvertible {
  private short r_;
  private short g_;
  private short b_;
  private short a_;


  public override string ToString()
    => $"rgba({this.r_}, {this.g_}, {this.b_}, {this.a_})";

  private static float ConvertS10ToFloat_(short s10)
    => s10.IsInRange<short>(0, 255) ? s10 / 255f : s10 / 1024f;

  public Color ToColor()
    => FinColor.FromRgbaFloats(
                   ConvertS10ToFloat_(this.r_),
                   ConvertS10ToFloat_(this.g_),
                   ConvertS10ToFloat_(this.b_),
                   ConvertS10ToFloat_(this.a_))
               .ToSystemColor();
}