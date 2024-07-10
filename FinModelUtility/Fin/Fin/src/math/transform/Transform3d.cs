using System.Numerics;

using schema.readOnly;

namespace fin.math.transform;

[GenerateReadOnly]
public partial interface ITransform3d
    : ITransform<Vector3, Quaternion?, Vector3?>;

public class Transform3d : ITransform3d {
  public Vector3 Translation { get; set; }
  public Quaternion? Rotation { get; set; } = Quaternion.Identity;
  public Vector3? Scale { get; set; } = Vector3.One;
}