using System;

using fin.model;
using fin.model.accessor;

namespace fin.shaders.glsl;

public interface IModelRequirements {
  public bool HasNormals { get; }
  public bool HasTangents { get; }

  public uint NumUvs { get; }
  public uint NumColors { get; }
  public uint NumBones { get; }
}

public class ModelRequirements : IModelRequirements {
  public static IModelRequirements FromModel(IReadOnlyModel model)
    => new ModelRequirements(model);

  private ModelRequirements(IReadOnlyModel model) {
    var vertexAccessor = MaximalVertexAccessor.GetAccessorForModel(model);
    foreach (var vertex in model.Skin.Vertices) {
      vertexAccessor.Target(vertex);

      this.HasNormals = this.HasNormals || vertexAccessor.LocalNormal != null;
      this.HasTangents
          = this.HasTangents || vertexAccessor.LocalTangent != null;

      this.NumBones = Math.Max(
          this.NumBones,
          (uint) (vertexAccessor.BoneWeights?.Weights.Count ?? 0));
      this.NumUvs = Math.Max(this.NumUvs, (uint) vertexAccessor.UvCount);
      this.NumColors
          = Math.Max(this.NumColors, (uint) vertexAccessor.ColorCount);
    }
  }

  public bool HasNormals { get; }
  public bool HasTangents { get; }

  public uint NumUvs { get; }
  public uint NumColors { get; }
  public uint NumBones { get; }
}