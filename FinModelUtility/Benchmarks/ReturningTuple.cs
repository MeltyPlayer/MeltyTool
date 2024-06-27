using BenchmarkDotNet.Attributes;

namespace benchmarks {
  public class ReturningTuple {
    private const int n = 100000;

    struct Xyzw {
      public float X { get; set; }
      public float Y { get; set; }
      public float Z { get; set; }
      public float W { get; set; }
    }

    [Benchmark]
    public void ReturnFromMethod() {
      for (var i = 0; i < n; i++) {
        var (value1, value2) = this.ReturnFromMethodImpl_();
      }
    }

    private (Xyzw, Xyzw) ReturnFromMethodImpl_()
      => (new Xyzw {X = 1, Y = 2, Z = 3, W = 4},
          new Xyzw { X = 2, Y = 3, Z = 4, W = 5 });


    [Benchmark]
    public void ReturnViaOut() {
      for (var i = 0; i < n; i++) {
        this.ReturnViaOutImpl_(out var value1, out var value2);
      }
    }

    private void ReturnViaOutImpl_(out Xyzw value1, out Xyzw value2) {
      value1 = new Xyzw { X = 1, Y = 2, Z = 3, W = 4 };
      value2 = new Xyzw { X = 2, Y = 3, Z = 4, W = 5 };
    }


    [Benchmark]
    public void ReturnFromRef() {
      for (var i = 0; i < n; i++) {
        Xyzw value1 = default, value2 = default;
        this.ReturnViaRefImpl_(ref value1, ref value2);
      }
    }

    private void ReturnViaRefImpl_(ref Xyzw value1, ref Xyzw value2) {
      value1 = new Xyzw { X = 1, Y = 2, Z = 3, W = 4 };
      value2 = new Xyzw { X = 2, Y = 3, Z = 4, W = 5 };
    }
  }
}