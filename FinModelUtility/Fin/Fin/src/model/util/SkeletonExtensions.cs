﻿using System.Numerics;

using fin.math.matrix.four;
using fin.math.xyz;
using fin.util.asserts;

namespace fin.model.util;

public static class SkeletonExtensions {
  public static IBone AddChild(this IBone parent, Vector3 position)
    => parent.AddChild(position.X, position.Y, position.Z);

  public static IBone AddChild(this IBone parent, IReadOnlyXyz position)
    => parent.AddChild(position.X, position.Y, position.Z);

  public static IBone AddChild(this IBone parent,
                               IReadOnlyFinMatrix4x4 matrix)
    => parent.AddChild(matrix.Impl);

  public static IBone AddChild(this IBone parent, Matrix4x4 matrix) {
    Asserts.True(Matrix4x4.Decompose(matrix,
                                     out var scale,
                                     out var rotation,
                                     out var translation) ||
                 !FinMatrix4x4.STRICT_DECOMPOSITION,
                 "Failed to decompose matrix!");

    var child = parent.AddChild(translation);
    child.LocalTransform.Rotation = rotation;
    child.LocalTransform.Scale = scale;

    return child;
  }

  public static Matrix4x4 GetWorldMatrix(this IBone bone) {
    var currentMatrix = Matrix4x4.Identity;
    while (bone != null) {
      currentMatrix = bone.LocalTransform.Matrix * currentMatrix;
      bone = bone.Parent;
    }
    return currentMatrix;
  }
}