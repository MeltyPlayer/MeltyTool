using System;

using fin.util.hash;

using gx;

using schema.binary;

namespace jsystem.schema.j3dgraph.bmd.mat3;

[BinarySchema]
public partial class TexCoordGen : ITexCoordGen, IBinaryConvertible {
  public GxTexGenType TexGenType { get; set; }
  public GxTexGenSrc TexGenSrc { get; set; }
  public GxTexMatrix TexMatrix { get; set; }
  private readonly byte padding_ = byte.MaxValue;

  public override string ToString()
    => $"TexCoordGen<{this.TexGenType}, {this.TexGenSrc}, {this.TexMatrix}>";

  public static bool operator ==(TexCoordGen lhs, TexCoordGen rhs)
    => lhs.Equals(rhs);

  public static bool operator !=(TexCoordGen lhs, TexCoordGen rhs)
    => !lhs.Equals(rhs);

  public override bool Equals(object? obj) {
      if (ReferenceEquals(this, obj)) {
        return true;
      }

      if (obj is TexCoordGen other) {
        return this.TexGenType == other.TexGenType &&
               this.TexGenSrc == other.TexGenSrc
               &&
               this.TexMatrix == other.TexMatrix;
      }

      return false;
    }

  public override int GetHashCode()
    => FluentHash.Start()
                 .With(this.TexGenType)
                 .With(this.TexGenSrc)
                 .With(this.TexMatrix)
                 .Hash;
}