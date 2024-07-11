using System.Numerics;

using fin.math.rotations;

using schema.readOnly;

namespace fin.math.transform;

[GenerateReadOnly]
public partial interface ITransform3d
    : ITransform<Vector3, Quaternion?, Vector3?>;

public class Transform3d : ITransform3d {
  private Quaternion? rotation_;
  private Vector3? eulerRadians_;

  public Vector3 Translation { get; set; }

  public Quaternion? Rotation {
    get => this.rotation_;
    set {
      this.rotation_ = value;
      this.eulerRadians_ = value?.ToEulerRadians();
    }
  }

  public Vector3? EulerRadians {
    get => this.eulerRadians_;
    set {
      this.eulerRadians_ = value;
      this.rotation_ = value?.CreateZyxRadians();
    }
  }

  public Vector3? Scale { get; set; }
}