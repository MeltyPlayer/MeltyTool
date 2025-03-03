using f3dzex2.combiner;

namespace f3dzex2.image;

/// <summary>
///   https://www.retroreversing.com/n64rdp
/// </summary>
public interface IRdp {
  ITmem Tmem { get; }

  uint PaletteSegmentedAddress { get; set; }

  CombinerCycleParams CombinerCycleParams0 { get; set; }
  CombinerCycleParams CombinerCycleParams1 { get; set; }
}

public class Rdp : IRdp {
  public ITmem Tmem { get; set; }

  public uint PaletteSegmentedAddress { get; set; }

  public CombinerCycleParams CombinerCycleParams0 { get; set; }
  public CombinerCycleParams CombinerCycleParams1 { get; set; }
}