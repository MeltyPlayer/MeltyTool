using System;
using System.Numerics;

using fin.math.xyz;
using fin.util.hash;

using schema.binary;

namespace fin.schema.vector;

public abstract class BVector3<T> {
  public T X { get; set; }
  public T Y { get; set; }
  public T Z { get; set; }

  public T this[int index] {
    get => index switch {
        0 => this.X,
        1 => this.Y,
        2 => this.Z,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
    };
    set {
      switch (index) {
        case 0: {
          this.X = value;
          break;
        }
        case 1: {
          this.Y = value;
          break;
        }
        case 2: {
          this.Z = value;
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }

  public void Set(T x, T y, T z) {
    this.X = x;
    this.Y = y;
    this.Z = z;
  }

  public override string ToString() => $"{{{this.X}, {this.Y}, {this.Z}}}";
}

[BinarySchema]
public sealed partial class Vector3f
    : BVector3<float>,
      IXyz,
      IBinaryConvertible {
  public static bool operator==(Vector3f? lhs, Vector3f? rhs)
    => lhs?.Equals(rhs) ?? (rhs == null);

  public static bool operator!=(Vector3f? lhs, Vector3f? rhs)
    => (!lhs?.Equals(rhs)) ?? (rhs != null);

  public override bool Equals(object? obj) {
    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj is Vector3f other) {
      return this.X == other.X &&
             this.Y == other.Y &&
             this.Z == other.Z;
    }

    return false;
  }

  public static explicit operator Vector3(Vector3f value)
    => new(value.X, value.Y, value.Z);

  public void Set(Vector3 xyz) {
    this.X = xyz.X;
    this.Y = xyz.Y;
    this.Z = xyz.Z;
  }

  public override int GetHashCode()
    => FluentHash.Start().With(this.X).With(this.Y).With(this.Z).Hash;
}

[BinarySchema]
public sealed partial class Vector3i : BVector3<int>, IBinaryConvertible;

[BinarySchema]
public sealed partial class Vector3s : BVector3<short>, IBinaryConvertible;

[BinarySchema]
public sealed partial class
    Vector3d : BVector3<double>, IBinaryConvertible;