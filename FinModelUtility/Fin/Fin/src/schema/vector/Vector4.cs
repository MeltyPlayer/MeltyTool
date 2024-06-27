using System;

using fin.model;

using schema.binary;

namespace fin.schema.vector;

public abstract class BVector4<T> {
  public T X { get; set; }
  public T Y { get; set; }
  public T Z { get; set; }
  public T W { get; set; }

  public T this[int index] {
    get => index switch {
        0 => X,
        1 => Y,
        2 => Z,
        3 => W,
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
        case 3: {
          this.W = value;
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }

  public void Set(T x, T y, T z, T w) {
    this.X = x;
    this.Y = y;
    this.Z = z;
    this.W = w;
  }

  public override string ToString() => $"{{{this.X}, {this.Y}, {this.Z}, {this.W}}}";
}

[BinarySchema]
public sealed partial class Vector4f 
    : BVector4<float>, IVector4, IBinaryConvertible { }