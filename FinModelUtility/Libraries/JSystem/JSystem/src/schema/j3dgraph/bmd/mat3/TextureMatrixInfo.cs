using System.Numerics;

using fin.util.hash;

using gx;

using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class TextureMatrixInfo
    : ITextureMatrixInfo,
      IBinaryConvertible {
  public GxTexGenType TexGenType { get; set; }
  public byte info;
  private readonly ushort padding1_ = ushort.MaxValue;
  public Vector3 Center { get; private set; }
  public Vector2 Scale { get; private set; }
  public short Rotation { get; set; }
  public readonly ushort padding2_ = ushort.MaxValue;
  public Vector2 Translation { get; private set; }
  public Matrix4x4 Matrix { get; private set; }


  public override string ToString()
    => $"TextureMatrixInfo<{this.TexGenType}, {this.Center}, {this.Scale}, {this.Translation}, {this.Rotation}, {this.Matrix}>";

  public static bool operator==(TextureMatrixInfo lhs, TextureMatrixInfo rhs)
    => lhs.Equals(rhs);

  public static bool operator!=(TextureMatrixInfo lhs, TextureMatrixInfo rhs)
    => !lhs.Equals(rhs);

  public override bool Equals(object? obj) {
    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj is TextureMatrixInfo other) {
      return this.TexGenType == other.TexGenType &&
             this.info == other.info &&
             this.Center == other.Center &&
             this.Scale == other.Scale &&
             this.Rotation == other.Rotation &&
             this.Translation == other.Translation &&
             this.Matrix == other.Matrix;
    }

    return false;
  }

  public override int GetHashCode()
    => FluentHash.Start()
                 .With(this.TexGenType)
                 .With(this.info)
                 .With(this.Center)
                 .With(this.Scale)
                 .With(this.Rotation)
                 .With(this.Translation)
                 .With(this.Matrix)
                 .Hash;
}