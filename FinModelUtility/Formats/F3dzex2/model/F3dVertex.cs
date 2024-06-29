using System.Numerics;

using schema.binary;

namespace f3dzex2.model;

[BinarySchema]
public partial record struct F3dVertex : IBinaryConvertible {
  public short X { get; set; }
  public short Y { get; set; }
  public short Z { get; set; }
  public Vector3 GetPosition() => new(X, Y, Z);

  public short Flag { get; set; }

  public short U { get; set; }
  public short V { get; set; }

  public Vector2 GetUv(float scaleX, float scaleY)
    => new(U * scaleX, V * scaleY);

  public byte NormalXOrR { get; set; }
  public byte NormalYOrG { get; set; }
  public byte NormalZOrB { get; set; }
  public byte A { get; set; }

  public Vector3 GetNormal() => new Vector3(
      GetNormalChannel_(NormalXOrR),
      GetNormalChannel_(NormalYOrG),
      GetNormalChannel_(NormalZOrB));

  private static float GetNormalChannel_(byte value)
    => ((sbyte) value) / (byte.MaxValue * .5f);

  public Vector4 GetColor()
    => new(NormalXOrR / 255f, NormalYOrG / 255f, NormalZOrB / 255f, A / 255f);
}