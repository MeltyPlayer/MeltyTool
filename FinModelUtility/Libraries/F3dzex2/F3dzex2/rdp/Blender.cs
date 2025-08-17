using fin.util.hash;


namespace f3dzex2.rdp;

public enum BlenderColorPm {
  G_BL_CLR_IN,
  G_BL_CLR_MEM,
  G_BL_CLR_BL,
  G_BL_CLR_FOG,
}

public enum BlenderAlphaA {
  G_BL_A_IN,
  G_BL_A_FOG,
  G_BL_A_SHADE,
  G_BL_0,
}

public enum BlenderAlphaB {
  G_BL_1MA,
  G_BL_A_MEM,
  G_BL_1,
  G_BL_0,
}

public struct BlenderCycleParams {
  public BlenderColorPm ColorP { get; set; }
  public BlenderColorPm ColorM { get; set; }

  public BlenderAlphaA AlphaA { get; set; }
  public BlenderAlphaB AlphaB { get; set; }

  public override int GetHashCode()
    => FluentHash.Start()
                 .With(this.ColorP)
                 .With(this.ColorM)
                 .With(this.AlphaA)
                 .With(this.AlphaB);

  public override bool Equals(object? other) {
    if (ReferenceEquals(this, other)) {
      return true;
    }

    if (other is BlenderCycleParams otherCombinerCycleParams) {
      return this.ColorP == otherCombinerCycleParams.ColorP &&
             this.ColorM == otherCombinerCycleParams.ColorM &&
             this.AlphaA == otherCombinerCycleParams.AlphaA &&
             this.AlphaB == otherCombinerCycleParams.AlphaB;
    }

    return false;
  }
}