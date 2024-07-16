using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;

using schema.readOnly;

namespace fin.math.transform;

[GenerateReadOnly]
public partial interface ITransform3d
    : ITransform<Vector3, Quaternion?, Vector3?> {
  public Matrix4x4 Matrix { get; }
  public Vector3? EulerRadians { get; set; }
}

public class Transform3d : ITransform3d {
  private Quaternion? rotation_;
  private Vector3? eulerRadians_;

  public Matrix4x4 Matrix
    => SystemMatrix4x4Util.FromTrs(this.Translation, this.Rotation, this.Scale);

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