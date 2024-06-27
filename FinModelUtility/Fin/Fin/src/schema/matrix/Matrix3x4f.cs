using schema.binary;

namespace fin.schema.matrix;

[BinarySchema]
public partial class Matrix3x4f : IBinaryConvertible {
  public float[] Values { get; } = new float[3 * 4];

  public float this[int row, int column] {
    get => this.Values[4 * row + column];
    set => this.Values[4 * row + column] = value;
  }
}