using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;
using fin.math.xyz;

namespace fin.model.util {
  public static class SkeletonExtensions {
    public static IBone AddChild(this IBone parent, Position position)
      => parent.AddChild(position.X, position.Y, position.Z);

    public static IBone AddChild(this IBone parent, Vector3 position)
      => parent.AddChild(position.X, position.Y, position.Z);

    public static IBone AddChild(this IBone parent, IReadOnlyXyz position)
      => parent.AddChild(position.X, position.Y, position.Z);

    public static IBone AddChild(this IBone parent, Matrix4x4 matrix) {
      Matrix4x4.Decompose(matrix,
                          out var scale,
                          out var quaternion,
                          out var position);
      return parent.AddChild(position)
                   .SetLocalRotation(quaternion)
                   .SetLocalScale(scale);
    }

    public static IBone AddChild(this IBone parent,
                                 IReadOnlyFinMatrix4x4 matrix) {
      matrix.Decompose(out var position, out var quaternion, out var scale);
      return parent.AddChild(position)
                   .SetLocalRotation(quaternion)
                   .SetLocalScale(scale);
    }


    public static IBone SetLocalPosition(this IBone parent, Position position)
      => parent.SetLocalPosition(position.X, position.Y, position.Z);

    public static IBone SetLocalPosition(this IBone parent, Vector3 position)
      => parent.SetLocalPosition(position.X, position.Y, position.Z);

    public static IBone SetLocalPosition(this IBone parent,
                                         IReadOnlyXyz position)
      => parent.SetLocalPosition(position.X, position.Y, position.Z);


    public static IBone SetLocalRotationRadians(this IBone parent,
                                                Vector3 eulerRadians)
      => parent.SetLocalRotationRadians(eulerRadians.X,
                                        eulerRadians.Y,
                                        eulerRadians.Z);

    public static IBone SetLocalRotationRadians(this IBone parent,
                                                IReadOnlyXyz eulerRadians)
      => parent.SetLocalRotationRadians(eulerRadians.X,
                                        eulerRadians.Y,
                                        eulerRadians.Z);


    public static IBone SetLocalRotationDegrees(this IBone parent,
                                                Vector3 eulerDegrees)
      => parent.SetLocalRotationDegrees(eulerDegrees.X,
                                        eulerDegrees.Y,
                                        eulerDegrees.Z);

    public static IBone SetLocalRotationDegrees(this IBone parent,
                                                IReadOnlyXyz eulerDegrees)
      => parent.SetLocalRotationDegrees(eulerDegrees.X,
                                        eulerDegrees.Y,
                                        eulerDegrees.Z);


    public static IBone SetLocalScale(this IBone parent, Scale scale)
      => parent.SetLocalScale(scale.X, scale.Y, scale.Z);

    public static IBone SetLocalScale(this IBone parent, Vector3 scale)
      => parent.SetLocalScale(scale.X, scale.Y, scale.Z);

    public static IBone SetLocalScale(this IBone parent, IReadOnlyXyz scale)
      => parent.SetLocalScale(scale.X, scale.Y, scale.Z);

    public static IBone SetLocalMatrix(this IBone bone,
                                       Matrix4x4 matrix) {
      Matrix4x4.Decompose(matrix,
                          out var scale,
                          out var quaternion,
                          out var position);
      return bone.SetLocalPosition(position)
                 .SetLocalRotation(quaternion)
                 .SetLocalScale(scale);
    }

    public static IBone SetLocalMatrix(this IBone bone,
                                       IReadOnlyFinMatrix4x4 matrix) {
      matrix.Decompose(out var position, out var quaternion, out var scale);
      return bone.SetLocalPosition(position)
                 .SetLocalRotation(quaternion)
                 .SetLocalScale(scale);
    }

    public static IBone SetLocalRotation(this IBone parent,
                                         Quaternion quaternion)
      => parent.SetLocalRotationRadians(
          QuaternionUtil.ToEulerRadians(quaternion));
  }
}