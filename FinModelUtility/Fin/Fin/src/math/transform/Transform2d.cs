using System.Numerics;

using schema.readOnly;

namespace fin.math.transform;

[GenerateReadOnly]
public partial interface ITransform2d : ITransform<Vector2, float, Vector2>;

public class Transform2d : ITransform2d {
  public Vector2 Translation { get; set; }
  public float Rotation { get; set; }
  public Vector2 Scale { get; set; } = Vector2.One;
}