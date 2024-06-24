using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;

namespace fin.model.util {
  public static class SkinExtensions {
    public static IBone AddChild(this IBone parent, Matrix4x4 matrix) {
      Matrix4x4.Decompose(matrix,
                          out var scale,
                          out var quaternion,
                          out var position);
      var rotation = QuaternionUtil.ToEulerRadians(quaternion);
      return parent.AddChild(position.X, position.Y, position.Z)
                   .SetLocalRotationRadians(
                       rotation.X,
                       rotation.Y,
                       rotation.Z)
                   .SetLocalScale(scale.X, scale.Y, scale.Z);
    }

    public static IBone AddChild(this IBone parent,
                                 IReadOnlyFinMatrix4x4 matrix) {
      matrix.Decompose(out var position,
                       out var quaternion,
                       out var scale);
      var rotation = QuaternionUtil.ToEulerRadians(quaternion);
      return parent.AddChild(position.X, position.Y, position.Z)
                   .SetLocalRotationRadians(
                       rotation.X,
                       rotation.Y,
                       rotation.Z)
                   .SetLocalScale(scale.X, scale.Y, scale.Z);
    }
  }
}