using f3dzex2.combiner;


namespace f3dzex2.image;

public static class RdpExtensions {
  public static void SetCombinerCycleParams(
      this IRdp rdp,
      (CombinerCycleParams, CombinerCycleParams?) combinerCycleParams)
    => rdp.SetCombinerCycleParams(combinerCycleParams.Item1,
                                  combinerCycleParams.Item2);

  public static void SetCombinerCycleParams(
      this IRdp rdp,
      CombinerCycleParams combinerCycleParams0,
      CombinerCycleParams? combinerCycleParams1 = null) {
    rdp.CycleType = combinerCycleParams1 != null
        ? CycleType.TWO_CYCLE
        : CycleType.ONE_CYCLE;
    rdp.CombinerCycleParams0 = combinerCycleParams0;
    if (combinerCycleParams1 != null) {
      rdp.CombinerCycleParams1 = combinerCycleParams1.Value;
    }
  }
}