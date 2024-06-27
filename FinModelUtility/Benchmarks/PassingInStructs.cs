using BenchmarkDotNet.Attributes;

namespace benchmarks {
  public class PassingInStructs {
    private const int n = 100000;

    struct XyzwStruct {
      public float X { get; set; }
      public float Y { get; set; }
      public float Z { get; set; }
      public float W { get; set; }
    }

    [Benchmark]
    public void PassingInNormally() {
      for (var i = 0; i < n; i++) {
        var value1 = new XyzwStruct { X = 1, Y = 2, Z = 3, W = 4 };
        var value2 = this.PassingInNormallyImpl_(value1);
      }
    }

    private XyzwStruct PassingInNormallyImpl_(XyzwStruct value) {
      return new XyzwStruct { X = 2, Y = 3, Z = 4, W = 5 };
    }

    [Benchmark]
    public void PassingInViaRef() {
      for (var i = 0; i < n; i++) {
        var value = new XyzwStruct { X = 1, Y = 2, Z = 3, W = 4 };
        this.PassingInViaRefImpl_(ref value);
      }
    }

    private void PassingInViaRefImpl_(ref XyzwStruct value) {
      value = new XyzwStruct { X = 2, Y = 3, Z = 4, W = 5 };
    }
  }
}