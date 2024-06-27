using BenchmarkDotNet.Attributes;

using CommunityToolkit.HighPerformance;


namespace benchmarks;

public class Loops {
  private const string TEXT =
      "sample foobar text, another line\nhere's some more text\0and there's more content after this\0";
  private readonly int n_ = 1000;

  private readonly List<int> values_ = Enumerable.Range(0, 10).ToList();

  [Benchmark]
  public void UsingForList() {
      for (var i = 0; i < n_; ++i) {
        for (var v = 0; v < values_.Count; ++v) {
          var value = values_[v];
        }
      }
    }

  [Benchmark]
  public void UsingForEachList() {
      for (var i = 0; i < n_; ++i) {
        foreach (var value in this.values_) {
        }
      }
    }

  [Benchmark]
  public void UsingForSpan() {
      for (var i = 0; i < n_; ++i) {
        var span = this.values_.AsSpan();
        for (var v = 0; v < span.Length; ++v) {
          var value = span[v];
        }
      }
    }

  [Benchmark]
  public void UsingForEachSpan() {
      for (var i = 0; i < n_; ++i) {
        foreach (var value in this.values_.AsSpan()) {
        }
      }
    }
}