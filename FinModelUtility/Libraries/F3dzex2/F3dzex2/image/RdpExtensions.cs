using f3dzex2.combiner;

using fin.math;


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

    var a = (uint) combinerCycleParams0.ColorMuxA;
    var b = (uint) combinerCycleParams0.ColorMuxB;
    var c = (uint) combinerCycleParams0.ColorMuxC;
    var d = (uint) combinerCycleParams0.ColorMuxD;
    var e = (uint) combinerCycleParams0.AlphaMuxA;
    var f = (uint) combinerCycleParams0.AlphaMuxB;
    var g = (uint) combinerCycleParams0.AlphaMuxC;
    var h = (uint) combinerCycleParams0.AlphaMuxD;

    var i = (uint) (combinerCycleParams1?.ColorMuxA ?? 0);
    var j = (uint) (combinerCycleParams1?.ColorMuxB ?? 0);
    var k = (uint) (combinerCycleParams1?.ColorMuxC ?? 0);
    var l = (uint) (combinerCycleParams1?.ColorMuxD ?? 0);
    var m = (uint) (combinerCycleParams1?.AlphaMuxA ?? 0);
    var n = (uint) (combinerCycleParams1?.AlphaMuxB ?? 0);
    var o = (uint) (combinerCycleParams1?.AlphaMuxC ?? 0);
    var p = (uint) (combinerCycleParams1?.AlphaMuxD ?? 0);

    var first = (uint) 0;
    first = first.SetFromRight(20, 4, a)
                 .SetFromRight(15, 5, c)
                 .SetFromRight(12, 3, e)
                 .SetFromRight(9, 3, g)
                 .SetFromRight(5, 4, i)
                 .SetFromRight(0, 5, k);
    var second = (uint) 0;
    second = second.SetFromRight(28, 4, b)
                   .SetFromRight(24, 4, j)
                   .SetFromRight(21, 3, m)
                   .SetFromRight(18, 3, o)
                   .SetFromRight(15, 3, d)
                   .SetFromRight(12, 3, f)
                   .SetFromRight(9, 3, h)
                   .SetFromRight(6, 3, l)
                   .SetFromRight(3, 3, n)
                   .SetFromRight(0, 3, p);

    rdp.CombinerCycleParams0 = combinerCycleParams0;
    if (combinerCycleParams1 != null) {
      rdp.CombinerCycleParams1 = combinerCycleParams1;
    }
  }
}