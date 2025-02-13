using System.Collections.Generic;
using System.Numerics;

using fin.data.indexable;
using fin.math.transform;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface ISkeleton : IEnumerable<IReadOnlyBone> {
  new IBone Root { get; }
  new IReadOnlyList<IBone> Bones { get; }
}

[GenerateReadOnly]
public partial interface ILeafBone : IIndexable {
  new string? Name { get; set; }

  new IBone Root { get; }
  new IBone? Parent { get; }

  new ITransform3d LocalTransform { get; }

  new bool IgnoreParentScale { get; set; }

  IBone AlwaysFaceTowardsCamera(Quaternion adjustment);
  new bool FaceTowardsCamera { get; }
  new Quaternion FaceTowardsCameraAdjustment { get; }
}

[GenerateReadOnly]
public partial interface IBone : ILeafBone {
  new IReadOnlyList<IBone> Children { get; }
  IBone AddRoot(float x, float y, float z);
  IBone AddChild(float x, float y, float z);
}