using System;

using fin.schema.matrix;
using fin.schema.vector;
using fin.util.hash;

using gx;

using schema.binary;

namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class TextureMatrixInfo : ITextureMatrixInfo,
                                         IBinaryConvertible {
  public GxTexGenType TexGenType { get; set; }
  public byte info;
  private readonly ushort padding1_ = ushort.MaxValue;
  public Vector3f Center { get; } = new();
  public Vector2f Scale { get; } = new();
  public short Rotation { get; set; }
  public readonly ushort padding2_ = ushort.MaxValue;
  public Vector2f Translation { get; } = new();
  public Matrix4x4f Matrix { get; } = new();


  public override string ToString()
    => $"TextureMatrixInfo<{TexGenType}, {Center}, {Scale}, {Translation}, {Rotation}, {Matrix}>";

  public static bool operator ==(TextureMatrixInfo lhs, TextureMatrixInfo rhs)
    => lhs.Equals(rhs);

  public static bool operator !=(TextureMatrixInfo lhs, TextureMatrixInfo rhs)
    => !lhs.Equals(rhs);

  public override bool Equals(object? obj) {
      if (Object.ReferenceEquals(this, obj)) {
        return true;
      }

      if (obj is TextureMatrixInfo other) {
        return this.TexGenType == other.TexGenType &&
               this.info == other.info &&
               Center == other.Center &&
               Scale == other.Scale &&
               Rotation == other.Rotation &&
               Translation == other.Translation &&
               Matrix == other.Matrix;
      }

      return false;
    }

  public override int GetHashCode()
    => FluentHash.Start()
                 .With(TexGenType)
                 .With(info)
                 .With(Center)
                 .With(Scale)
                 .With(Rotation)
                 .With(Translation)
                 .With(Matrix)
                 .Hash;
}