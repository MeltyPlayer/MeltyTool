using System;

using fin.util.hash;

using schema.binary;

namespace fin.schema.matrix;

[BinarySchema]
public partial class Matrix4x4f : IBinaryConvertible {
  public float[] Values { get; } = new float[4 * 4];

  public float this[int row, int column] {
    get => this.Values[4 * row + column];
    set => this.Values[4 * row + column] = value;
  }

  public static bool operator ==(Matrix4x4f lhs, Matrix4x4f rhs)
    => lhs.Equals(rhs);

  public static bool operator !=(Matrix4x4f lhs, Matrix4x4f rhs)
    => !lhs.Equals(rhs);

  public override bool Equals(object? obj) {
    if (Object.ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj is Matrix4x4f other) {
      for (var i = 0; i < this.Values.Length; ++i) {
        if (Values[i] != other.Values[i]) {
          return false;
        }
      }

      return true;
    }

    return false;
  }

  public override int GetHashCode() {
    var hash = FluentHash.Start();

    foreach (var value in Values) {
      hash = hash.With(value);
    }

    return hash.Hash;
  }
}