using BenchmarkDotNet.Attributes;

namespace benchmarks {
  public class ReadingStringNts {
    private const int n = 100000;

    [Benchmark]
    public void ReadUntilNull() {
      for (var i = 0; i < n; i++) {
      }
    }
  }
}