using f3dzex2.combiner;

using fin.math;

using Tedd;


namespace f3dzex2.image;

/// <summary>
///   https://www.retroreversing.com/n64rdp
/// </summary>
public interface IRdp {
  ITmem Tmem { get; }

  uint PaletteSegmentedAddress { get; set; }

  uint OtherModeH { get; set; }
  CycleType CycleType {
    get => (CycleType) this.OtherModeH.ExtractFromRight(20, 2);
    set {
      var bit0 = ((uint) value).GetBit(0);
      var bit1 = ((uint) value).GetBit(1);

      var otherModeH = this.OtherModeH;
      otherModeH.SetBit(20, bit0);
      otherModeH.SetBit(21, bit1);

      this.OtherModeH = otherModeH;
    }
  }

  uint OtherModeL { get; set; }

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