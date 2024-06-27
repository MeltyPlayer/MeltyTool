using System.Numerics;

using fin.util.hash;

using schema.binary;

namespace f3dzex2.model;

[BinarySchema]
public partial struct F3dVertex : IBinaryConvertible {
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

  public override int GetHashCode() => FluentHash.Start()
                                                 .With(X)
                                                 .With(Y)
                                                 .With(Z)
                                                 .With(Flag)
                                                 .With(U)
                                                 .With(V)
                                                 .With(NormalXOrR)
                                                 .With(NormalYOrG)
                                                 .With(NormalZOrB)
                                                 .With(A);

  public override bool Equals(object? other) {
    if (object.ReferenceEquals(this, other)) {
      return true;
    }

    if (other is F3dVertex otherVertex) {
      return this.X == otherVertex.X && 
             this.Y == otherVertex.Y &&
             this.Z == otherVertex.Z && 
             this.Flag == otherVertex.Flag &&
             this.U == otherVertex.U && 
             this.V == otherVertex.V &&
             this.NormalXOrR == otherVertex.NormalXOrR && 
             this.NormalYOrG == otherVertex.NormalYOrG &&
             this.NormalZOrB == otherVertex.NormalZOrB &&
             this.A == otherVertex.A;
    }

    return false;
  }
}