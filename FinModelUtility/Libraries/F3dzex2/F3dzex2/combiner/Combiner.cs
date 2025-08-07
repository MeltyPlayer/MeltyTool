﻿using fin.util.hash;

namespace f3dzex2.combiner;

public enum GenericColorMux {
  G_CCMUX_COMBINED,
  G_CCMUX_TEXEL0,
  G_CCMUX_TEXEL1,
  G_CCMUX_PRIMITIVE,
  G_CCMUX_SHADE,
  G_CCMUX_ENVIRONMENT,
  G_CCMUX_1,
  G_CCMUX_0,
  G_CCMUX_NOISE,
  G_CCMUX_CENTER,
  G_CCMUX_K4,

  G_CCMUX_COMBINED_ALPHA,
  G_CCMUX_TEXEL0_ALPHA,
  G_CCMUX_TEXEL1_ALPHA,
  G_CCMUX_PRIMITIVE_ALPHA,
  G_CCMUX_SHADE_ALPHA,
  G_CCMUX_ENV_ALPHA,
  G_CCMUX_LOD_FRAC,
  G_CCMUX_PRIM_LOD_FRAC,
  G_CCMUX_SCALE,
  G_CCMUX_K5,
}

public enum GenericAlphaMux {
  G_ACMUX_COMBINED,
  G_ACMUX_TEXEL0,
  G_ACMUX_TEXEL1,
  G_ACMUX_PRIMITIVE,
  G_ACMUX_SHADE,
  G_ACMUX_ENVIRONMENT,
  G_ACMUX_1,
  G_ACMUX_0,

  G_ACMUX_PRIM_LOD_FRAC,
  G_ACMUX_LOD_FRACTION,
}

public struct CombinerCycleParams {
  public GenericColorMux ColorMuxA { get; set; }
  public GenericColorMux ColorMuxB { get; set; }
  public GenericColorMux ColorMuxC { get; set; }
  public GenericColorMux ColorMuxD { get; set; }

  public GenericAlphaMux AlphaMuxA { get; set; }
  public GenericAlphaMux AlphaMuxB { get; set; }
  public GenericAlphaMux AlphaMuxC { get; set; }
  public GenericAlphaMux AlphaMuxD { get; set; }

  public override int GetHashCode()
    => FluentHash.Start()
                 .With(this.ColorMuxA)
                 .With(this.ColorMuxB)
                 .With(this.ColorMuxC)
                 .With(this.ColorMuxD)
                 .With(this.AlphaMuxA)
                 .With(this.AlphaMuxB)
                 .With(this.AlphaMuxC)
                 .With(this.AlphaMuxD);

  public override bool Equals(object? other) {
    if (ReferenceEquals(this, other)) {
      return true;
    }

    if (other is CombinerCycleParams otherCombinerCycleParams) {
      return this.ColorMuxA == otherCombinerCycleParams.ColorMuxA &&
             this.ColorMuxB == otherCombinerCycleParams.ColorMuxB &&
             this.ColorMuxC == otherCombinerCycleParams.ColorMuxC &&
             this.ColorMuxD == otherCombinerCycleParams.ColorMuxD &&
             this.AlphaMuxA == otherCombinerCycleParams.AlphaMuxA &&
             this.AlphaMuxB == otherCombinerCycleParams.AlphaMuxB &&
             this.AlphaMuxC == otherCombinerCycleParams.AlphaMuxC &&
             this.AlphaMuxD == otherCombinerCycleParams.AlphaMuxD;
    }

    return false;
  }
}