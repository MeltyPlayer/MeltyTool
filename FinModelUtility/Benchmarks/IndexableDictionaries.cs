using BenchmarkDotNet.Attributes;

using fin.data.indexable;

namespace benchmarks;

public struct Indexable : IIndexable {
  public required int Index { get; init; }
}

public class IndexableDictionaries {
  private const int X_ = 10000;
  private const int N_ = 1000;

  private readonly SparseIndexableDictionary<Indexable, int> indexableV1_ = new(X_);

  [GlobalSetup]
  public void SetUp() {
    for (var i = 0; i < X_; i++) {
      this.indexableV1_[i] = i;
    }
  }

  [Benchmark]
  public void AddWithoutCapacityV1() {
    for (var iteration = 0; iteration < N_; ++iteration) {
      var impl = new SparseIndexableDictionary<Indexable, int>();
      for (var i = 0; i < X_; i++) {
        impl[i] = i;
      }
    }
  }

  [Benchmark]
  public void AddWithCapacityV1() {
    for (var iteration = 0; iteration < N_; ++iteration) {
      var impl = new SparseIndexableDictionary<Indexable, int>(X_);
      for (var i = 0; i < X_; i++) {
        impl[i] = i;
      }
    }
  }

  [Benchmark]
  public void EnumerateV1() {
    for (var iteration = 0; iteration < N_; ++iteration) {
      foreach (var value in this.indexableV1_) {
        var foo = value;
      }
    }
  }

  [Benchmark]
  public void TryGetValueV1() {
    for (var iteration = 0; iteration < N_; ++iteration) {
      for (var i = 0; i < X_; i++) {
        if (this.indexableV1_.TryGetValue(i, out var value)) {
          var foo = value;
        }
      }
    }
  }
}