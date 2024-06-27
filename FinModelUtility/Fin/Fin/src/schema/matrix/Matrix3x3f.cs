using schema.binary;

namespace fin.schema.matrix;

[BinarySchema]
public partial class Matrix3x3f : IBinaryConvertible {
  public float[] Values { get; } = new float[3 * 3];

  public float this[int row, int column] {
    get => this.Values[3 * row + column];
    set => this.Values[3 * row + column] = value;
  }
}