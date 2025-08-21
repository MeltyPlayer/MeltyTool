using f3dzex2.combiner;

using fin.math;

using Tedd;


namespace f3dzex2.image;

public enum ZMode {
  ZMODE_OPA,
  ZMODE_INTER,
  ZMODE_XLU,
  ZMODE_DEC,
}

public enum BlenderPm {
  G_BL_CLR_IN,
  G_BL_CLR_MEM,
  G_BL_CLR_BL,
  G_BL_CLR_FOG,
}

public enum BlenderA {
  G_BL_A_IN,
  G_BL_A_FOG,
  G_BL_A_SHADE,
  G_BL_0,
}

public enum BlenderB {
  G_BL_1MA,
  G_BL_A_MEM,
  G_BL_1,
  G_BL_0,
}

/// <summary>
///   https://www.retroreversing.com/n64rdp
/// </summary>
public interface IRdp {
  ITmem Tmem { get; }

  uint PaletteSegmentedAddress { get; set; }

  uint OtherModeH { get; set; }

  CycleType CycleType {
    get => (CycleType) this.OtherModeH.ExtractFromRight(20, 2);
    set => this.OtherModeH = this.OtherModeH.SetFromRight(20, 2, (uint) value);
  }

  uint OtherModeL { get; set; }

  // Cycle-Independent Blender Settings
  bool ZCompare {
    get => this.OtherModeL.GetBit(3 + 1);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(3 + 1, 1, (uint) (value ? 1 : 0));
  }

  bool ZUpdate {
    get => this.OtherModeL.GetBit(3 + 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(3 + 2, 1, (uint) (value ? 1 : 0));
  }

  ZMode ZMode {
    get => (ZMode) this.OtherModeL.ExtractFromRight(3 + 7, 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(3 + 7, 2, (uint) value);
  }

  bool MultiplyCoverageWithAlpha {
    get => this.OtherModeL.GetBit(3 + 9);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(3 + 9, 1, (uint) (value ? 1 : 0));
  }

  bool UseCoverageForAlpha {
    get => this.OtherModeL.GetBit(3 + 10);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(3 + 10, 1, (uint) (value ? 1 : 0));
  }

  bool ForceBlending {
    get => this.OtherModeL.GetBit(3 + 11);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(3 + 11, 1, (uint) (value ? 1 : 0));
  }


  // Cycle-dependent Blender Settings
  BlenderPm P0 {
    get => (BlenderPm) this.OtherModeL.ExtractFromRight(16 + 14, 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(16 + 14, 2, (uint) value);
  }

  BlenderPm P1 {
    get => (BlenderPm) this.OtherModeL.ExtractFromRight(16 + 12, 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(16 + 12, 2, (uint) value);
  }

  BlenderA A0 {
    get => (BlenderA) this.OtherModeL.ExtractFromRight(16 + 10, 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(16 + 10, 2, (uint) value);
  }

  BlenderA A1 {
    get => (BlenderA) this.OtherModeL.ExtractFromRight(16 + 8, 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(16 + 8, 2, (uint) value);
  }

  BlenderPm M0 {
    get => (BlenderPm) this.OtherModeL.ExtractFromRight(16 + 6, 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(16 + 6, 2, (uint) value);
  }

  BlenderPm M1 {
    get => (BlenderPm) this.OtherModeL.ExtractFromRight(16 + 4, 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(16 + 4, 2, (uint) value);
  }

  BlenderB B0 {
    get => (BlenderB) this.OtherModeL.ExtractFromRight(16 + 2, 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(16 + 2, 2, (uint) value);
  }

  BlenderB B1 {
    get => (BlenderB) this.OtherModeL.ExtractFromRight(16 + 0, 2);
    set => this.OtherModeL =
        this.OtherModeL.SetFromRight(16 + 0, 2, (uint) value);
  }

  CombinerCycleParams CombinerCycleParams0 { get; set; }
  CombinerCycleParams CombinerCycleParams1 { get; set; }
}

public class Rdp : IRdp {
  public ITmem Tmem { get; set; }

  public uint PaletteSegmentedAddress { get; set; }

  public uint OtherModeH { get; set; }
  public uint OtherModeL { get; set; }

  public CombinerCycleParams CombinerCycleParams0 { get; set; }
  public CombinerCycleParams CombinerCycleParams1 { get; set; }
}