using System.Numerics;

using fin.color;
using fin.schema.color;

using OneOf;

namespace gx.displayList;

[GenerateOneOf]
public partial class IndexOrColor : OneOfBase<ushort, Rgba32>;

public class GxVertex {
  public ushort? PositionIndex { get; set; }
  public ushort? JointIndex { get; set; }
  public ushort? NormalIndex { get; set; }
  public ushort? NbtIndex { get; set; }
  public IndexOrColor? Color0IndexOrValue { get; set; }
  public IndexOrColor? Color1IndexOrValue { get; set; }
  public ushort? TexCoord0Index { get; set; }
  public ushort? TexCoord1Index { get; set; }
  public ushort? TexCoord2Index { get; set; }
  public ushort? TexCoord3Index { get; set; }
  public ushort? TexCoord4Index { get; set; }
  public ushort? TexCoord5Index { get; set; }
  public ushort? TexCoord6Index { get; set; }
  public ushort? TexCoord7Index { get; set; }
}