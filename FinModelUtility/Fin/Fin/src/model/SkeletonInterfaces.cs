using System.Collections.Generic;
using System.Numerics;

using fin.data.indexable;

using schema.readOnly;

namespace fin.model {
  public interface ISkeleton : IEnumerable<IBone> {
    IBone Root { get; }
    IReadOnlyList<IBone> Bones { get; }
  }

  [GenerateReadOnly]
  public partial interface ILeafBone : IIndexable {
    string Name { get; set; }

    IBone Root { get; }
    IBone? Parent { get; }

    Position LocalPosition { get; }
    IRotation? LocalRotation { get; }
    Scale? LocalScale { get; }

    IBone SetLocalPosition(float x, float y, float z);
    IBone SetLocalRotationDegrees(float x, float y, float z);
    IBone SetLocalRotationRadians(float x, float y, float z);
    IBone SetLocalScale(float x, float y, float z);

    bool IgnoreParentScale { get; set; }

    IBone AlwaysFaceTowardsCamera(Quaternion adjustment);
    bool FaceTowardsCamera { get; }
    Quaternion FaceTowardsCameraAdjustment { get; }
  }

  [GenerateReadOnly]
  public partial interface IBone : ILeafBone {
    IReadOnlyList<IBone> Children { get; }
    IBone AddRoot(float x, float y, float z);
    IBone AddChild(float x, float y, float z);
  }
}