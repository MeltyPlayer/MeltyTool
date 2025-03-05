using System;
using System.Collections.Generic;
using System.Numerics;

using fin.scene;


namespace fin.model.util;

public class SceneModelMinMaxBoundsScaleCalculator
    : BMinMaxBoundsScaleCalculator<ISceneModel> {
  private readonly BoneTransformManager boneTransformManager_ = new();

  public override Bounds CalculateBounds(ISceneModel sceneModel) {
    var minX = float.MaxValue;
    var minY = float.MaxValue;
    var minZ = float.MaxValue;
    var maxX = float.MinValue;
    var maxY = float.MinValue;
    var maxZ = float.MinValue;

    this.FactorModelIntoBounds_(sceneModel,
                                ref minX,
                                ref minY,
                                ref minZ,
                                ref maxX,
                                ref maxY,
                                ref maxZ);

    return new Bounds(minX, minY, minZ, maxX, maxY, maxZ);
  }

  public Bounds CalculateBounds(IEnumerable<ISceneModel> sceneModels) {
    var minX = float.MaxValue;
    var minY = float.MaxValue;
    var minZ = float.MaxValue;
    var maxX = float.MinValue;
    var maxY = float.MinValue;
    var maxZ = float.MinValue;

    foreach (var sceneModel in sceneModels) {
      this.FactorModelIntoBounds_(sceneModel,
                                  ref minX,
                                  ref minY,
                                  ref minZ,
                                  ref maxX,
                                  ref maxY,
                                  ref maxZ);
    }

    return new Bounds(minX, minY, minZ, maxX, maxY, maxZ);
  }

  private void FactorModelIntoBounds_(ISceneModel sceneModel,
                                      ref float minX,
                                      ref float minY,
                                      ref float minZ,
                                      ref float maxX,
                                      ref float maxY,
                                      ref float maxZ) {
    var model = sceneModel.Model;

    this.boneTransformManager_.CalculateStaticMatricesForManualProjection(model);

    var anyVertices = false;
    foreach (var vertex in model.Skin.Vertices) {
      anyVertices = true;

      this.boneTransformManager_.ProjectVertexPosition(
          vertex,
          out var position);

      var x = position.X;
      var y = position.Y;
      var z = position.Z;

      minX = MathF.Min(minX, x);
      maxX = MathF.Max(maxX, x);

      minY = MathF.Min(minY, y);
      maxY = MathF.Max(maxY, y);

      minZ = MathF.Min(minZ, z);
      maxZ = MathF.Max(maxZ, z);
    }

    if (!anyVertices) {
      var boneQueue = new Queue<IReadOnlyBone>();
      boneQueue.Enqueue(model.Skeleton.Root);

      while (boneQueue.Count > 0) {
        var bone = boneQueue.Dequeue();

        var xyz = new Vector3();
        this.boneTransformManager_.ProjectPosition(bone, ref xyz);

        minX = MathF.Min(minX, xyz.X);
        maxX = MathF.Max(maxX, xyz.X);

        minY = MathF.Min(minY, xyz.Y);
        maxY = MathF.Max(maxY, xyz.Y);

        minZ = MathF.Min(minZ, xyz.Z);
        maxZ = MathF.Max(maxZ, xyz.Z);

        foreach (var child in bone.Children) {
          boneQueue.Enqueue(child);
        }
      }
    }
  }
}