using BenchmarkDotNet.Attributes;

namespace benchmarks {
  public class Finite {
    private const int n = 10000000;

    [Benchmark]
    public void IsFiniteMethod() {
      for (var i = 0; i < n; i++) {
        var f = 1f * i;
        var a = float.IsFinite(f);
      }
    }

    [Benchmark]
    public void IsFiniteManual() {
      for (var i = 0; i < n; i++) {
        var f = 1f * i;
        var a = float.IsNaN(f) && float.IsInfinity(f);
      }
    }
  }
}