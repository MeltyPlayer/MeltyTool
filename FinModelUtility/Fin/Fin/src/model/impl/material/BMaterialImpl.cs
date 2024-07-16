using System.Collections.Generic;

using fin.util.image;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private abstract class BMaterialImpl : IMaterial {
    public abstract IEnumerable<IReadOnlyTexture> Textures { get; }

    public string? Name { get; set; }
    public CullingMode CullingMode { get; set; }

    public DepthMode DepthMode { get; set; } = DepthMode.READ_AND_WRITE;

    public DepthCompareType DepthCompareType { get; set; }
      = DepthCompareType.LEqual;

    public bool IgnoreLights { get; set; }

    public float Shininess { get; set; } =
      MaterialConstants.DEFAULT_SHININESS;

    public TransparencyType TransparencyType { get; set; }
      = TransparencyType.MASK;

    public bool UpdateColorChannel { get; set; } = true;
    public bool UpdateAlphaChannel { get; set; } = true;
  }
}