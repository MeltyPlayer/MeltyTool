﻿using System.Collections.Generic;

using fin.color;
using fin.model.impl.material;
using fin.image.util;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private abstract class BMaterialImpl : IMaterial {
    public abstract IEnumerable<IReadOnlyTexture> Textures { get; }

    public string? Name { get; set; }
    public CullingMode CullingMode { get; set; }

    public DepthMode DepthMode { get; set; } = DepthMode.READ_AND_WRITE;

    public DepthCompareType DepthCompareType { get; set; }
      = DepthCompareType.Less;

    public bool IgnoreLights { get; set; }

    public float Shininess { get; set; } =
      MaterialConstants.DEFAULT_SHININESS;

    public TransparencyType TransparencyType { get; set; }
      = TransparencyType.MASK;

    public bool UpdateColorChannel { get; set; } = true;
    public bool UpdateAlphaChannel { get; set; } = true;

    public IFogParams? FogParams { get; private set; }

    public IMaterial SetFog(float nearDistance,
                            float farDistance,
                            IColor color) {
      this.FogParams = new FogParams {
          NearDistance = nearDistance,
          FarDistance = farDistance,
          Color = color
      };
      return this;
    }

    public IMaterial SetBlending(
        BlendEquation blendEquation,
        BlendFactor srcFactor,
        BlendFactor dstFactor,
        LogicOp logicOp)
      => this.SetBlendingSeparate(blendEquation,
                                  srcFactor,
                                  dstFactor,
                                  blendEquation,
                                  srcFactor,
                                  dstFactor,
                                  logicOp);

    public IMaterial SetBlendingSeparate(
        BlendEquation colorBlendEquation,
        BlendFactor colorSrcFactor,
        BlendFactor colorDstFactor,
        BlendEquation alphaBlendEquation,
        BlendFactor alphaSrcFactor,
        BlendFactor alphaDstFactor,
        LogicOp logicOp) {
      this.ColorBlendEquation = colorBlendEquation;
      this.ColorSrcFactor = colorSrcFactor;
      this.ColorDstFactor = colorDstFactor;
      this.AlphaBlendEquation = alphaBlendEquation;
      this.AlphaSrcFactor = alphaSrcFactor;
      this.AlphaDstFactor = alphaDstFactor;
      this.LogicOp = logicOp;
      return this;
    }

    public BlendEquation ColorBlendEquation { get; private set; } =
      BlendEquation.ADD;

    public BlendFactor ColorSrcFactor { get; private set; } =
      BlendFactor.SRC_ALPHA;

    public BlendFactor ColorDstFactor { get; private set; } =
      BlendFactor.ONE_MINUS_SRC_ALPHA;

    public BlendEquation AlphaBlendEquation { get; private set; } =
      BlendEquation.ADD;

    public BlendFactor AlphaSrcFactor { get; private set; } =
      BlendFactor.ONE;

    public BlendFactor AlphaDstFactor { get; private set; } =
      BlendFactor.ONE;

    public LogicOp LogicOp { get; private set; } = LogicOp.UNDEFINED;
  }
}