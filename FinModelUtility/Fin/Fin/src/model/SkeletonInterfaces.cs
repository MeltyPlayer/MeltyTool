using System.Collections.Generic;
using System.Numerics;

using fin.data.indexable;
using fin.math.transform;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface ISkeleton : IEnumerable<IReadOnlyBone> {
  IBone Root { get; }
  IReadOnlyList<IBone> Bones { get; }
}

[GenerateReadOnly]
public partial interface ILeafBone : IIndexable {
  string Name { get; set; }

  IBone Root { get; }
  IBone? Parent { get; }

  IBoneTransform LocalTransform { get; }

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