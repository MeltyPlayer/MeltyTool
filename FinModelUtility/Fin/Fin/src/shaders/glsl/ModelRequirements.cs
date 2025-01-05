using System;
using System.Linq;

using fin.model;
using fin.model.accessor;
using fin.util.enumerables;

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
    var skin = model.Skin;

    switch (skin) {
      case ISkin<INormalVertex> normalSkin: {
        this.HasNormals
            = normalSkin.TypedVertices.Any(v => v.LocalNormal != null);
        break;
      }
    }

    switch (skin) {
      case ISkin<ITangentVertex> tangentSkin: {
        this.HasTangents
            = tangentSkin.TypedVertices.Any(v => v.LocalTangent != null);
        break;
      }
    }

    switch (skin) {
      case ISkin<IMultiColorVertex> multiColorSkin: {
        this.NumColors
            = (uint) multiColorSkin.TypedVertices.MaxOrDefault(v => v.ColorCount);
        break;
      }
      case ISkin<ISingleColorVertex> singleColorSkin: {
        this.NumColors
            = singleColorSkin.TypedVertices.Any(v => v.GetColor() != null)
                ? (uint) 1
                : 0;
        break;
      }
    }

    switch (skin) {
      case ISkin<IMultiUvVertex> multiUvSkin: {
        this.NumUvs = (uint) multiUvSkin.TypedVertices.MaxOrDefault(v => v.UvCount);
        break;
      }
      case ISkin<ISingleUvVertex> singleUvSkin: {
        this.NumUvs = singleUvSkin.TypedVertices.Any(v => v.GetUv() != null)
            ? (uint) 1
            : 0;
        break;
      }
    }

    this.NumBones = (uint) skin.BoneWeights.MaxOrDefault(b => b.Weights.Count);
  }

  public bool HasNormals { get; }
  public bool HasTangents { get; }

  public uint NumUvs { get; }
  public uint NumColors { get; }
  public uint NumBones { get; }
}