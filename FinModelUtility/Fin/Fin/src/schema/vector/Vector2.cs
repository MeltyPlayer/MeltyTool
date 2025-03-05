using System;

using fin.model;
using fin.util.hash;

using schema.binary;

namespace fin.schema.vector;

public abstract class BVector2<T> {
  public T X { get; set; }
  public T Y { get; set; }

  public T this[int index] {
    get => index switch {
        0 => this.X,
        1 => this.Y,
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
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }

  public void Set(T x, T y) {
    this.X = x; 
    this.Y = y;
  }

  public override string ToString() => $"{{{this.X}, {this.Y}}}";
}

[BinarySchema]
public sealed partial class Vector2f
    : BVector2<float>, IVector2, IBinaryConvertible {
  public static bool operator ==(Vector2f lhs, Vector2f rhs)
    => lhs.Equals(rhs);

  public static bool operator !=(Vector2f lhs, Vector2f rhs)
    => !lhs.Equals(rhs);

  public override bool Equals(object? obj) {
    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj is Vector2f other) {
      return this.X == other.X && this.Y == other.Y;
    }

    return false;
  }

  public override int GetHashCode()
    => FluentHash.Start().With(this.X).With(this.Y).Hash;
}

[BinarySchema]
public sealed partial class Vector2i : BVector2<int>, IBinaryConvertible;

[BinarySchema]
public sealed partial class Vector2s : BVector2<short>, IBinaryConvertible;