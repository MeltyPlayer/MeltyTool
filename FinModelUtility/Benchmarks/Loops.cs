using System.Numerics;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using CommunityToolkit.HighPerformance;


namespace benchmarks {
  public class Loops {
    private readonly int n_ = 100;

    private readonly Vector3[] values_
        = Enumerable.Range(0, 100).Select(_ => new Vector3()).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ProcessValue(in Vector3 value) {
      return value + Vector3.One;
    }

    [Benchmark]
    public void NotPossible_UsingForList() {
      for (var i = 0; i < this.n_; ++i) {
        for (var v = 0; v < this.values_.Length; ++v) {
          this.ProcessValue(this.values_[v]);
        }
      }
    }

    [Benchmark]
    public void NotPossible_UsingForEachList() {
      for (var i = 0; i < this.n_; ++i) {
        foreach (var value in this.values_) {
          this.ProcessValue(value);
        }
      }
    }

    [Benchmark]
    public void NotPossible_UsingForSpan() {
      for (var i = 0; i < this.n_; ++i) {
        var span = this.values_.AsSpan();
        for (var v = 0; v < span.Length; ++v) {
          this.ProcessValue(span[v]);
        }
      }
    }

    [Benchmark]
    public void NotPossible_UsingForEachSpan() {
      for (var i = 0; i < this.n_; ++i) {
        foreach (var value in this.values_.AsSpan()) {
          this.ProcessValue(value);
        }
      }
    }

    [Benchmark]
    public void Possible_ViaSpanHandler() {
      for (var i = 0; i < this.n_; ++i) {
        this.ForEach(
            buffer => this.ProcessValue(buffer.Cast<float, Vector3>()[0]));
      }
    }

    public delegate void ForEachHandler(Span<float> span);

    public void ForEach(ForEachHandler handler) {
      var span = this.values_.AsSpan().Cast<Vector3, float>();
      for (var i = 0; i + 3 <= span.Length; i += 3) {
        handler(span.Slice(i, 3));
      }
    }

    [Benchmark]
    public void Possible_ViaSpanHandlerGeneric() {
      for (var i = 0; i < this.n_; ++i) {
        this.ForEach<Vector3>(buffer => this.ProcessValue(buffer));
      }
    }

    public delegate void ForEachHandler<in T>(T span);

    public unsafe void ForEach<T>(ForEachHandler<T> handler) where T : unmanaged {
      var span = this.values_.AsSpan().Cast<Vector3, T>();
      var size = sizeof(T);
      foreach (var t in span) {
        handler(t);
      }
    }

    [Benchmark]
    public void Possible_ViaSpanHandlerSlow() {
      for (var i = 0; i < this.n_; ++i) {
        this.ForEachSlow(
            buffer => this.ProcessValue(buffer.Cast<float, Vector3>()[0]));
      }
    }

    public void ForEachSlow(ForEachHandler handler) {
      Memory<float> memory = this.values_.AsMemory().Cast<Vector3, float>();
      for (var i = 0; i + 3 <= memory.Length; i += 3) {
        var span = memory.Span;
        handler(span.Slice(i, 3));
      }
    }

    [Benchmark]
    public void Possible_ViaVector3Handler() {
      for (var i = 0; i < this.n_; ++i) {
        this.ForEach(t => this.ProcessValue(t));
      }
    }

    public void ForEach(Action<Vector3> handler) {
      foreach (var t in this.values_) {
        handler(t);
      }
    }

    [Benchmark]
    public void Possible_ViaEnumerable() {
      for (var i = 0; i < this.n_; ++i) {
        foreach (var t in this.EnumerateItems()) {
          this.ProcessValue(t);
        }
      }
    }

    public IEnumerable<Vector3> EnumerateItems() {
      foreach (var t in this.values_) {
        yield return t;
      }
    }
  }
}