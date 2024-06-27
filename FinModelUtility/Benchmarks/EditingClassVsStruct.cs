using BenchmarkDotNet.Attributes;

namespace benchmarks;

public class EditingClassVsStruct {
  private const int n = 100000;

  interface IXyzw {
    float X { get; set; }
    float Y { get; set; }
    float Z { get; set; }
    float W { get; set; }
  }

  struct XyzwStruct : IXyzw {
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }
  }

  class XyzwClass : IXyzw {
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }
  }

  [Benchmark]
  public void EditingClass() {
      for (var i = 0; i < n; i++) {
      var value = new XyzwClass();
        this.EditingClassImpl_(value);
      }
    }

  private void EditingClassImpl_(XyzwClass value) {
      value.X = 1;
      value.Y = 2;
      value.Z = 3;
      value.W = 4;
    }

  [Benchmark]
  public void EditingClassViaInterface() {
      var value = new XyzwClass();
      for (var i = 0; i < n; i++) {
        this.EditingClassViaInterfaceImpl_(value);
      }
    }

  private void EditingClassViaInterfaceImpl_(IXyzw value) {
      value.X = 1;
      value.Y = 2;
      value.Z = 3;
      value.W = 4;
    }

  [Benchmark]
  public void EditingStruct() {
      for (var i = 0; i < n; i++) {
        this.EditingStructImpl_(out var value);
      }
    }

  private void EditingStructImpl_(out XyzwStruct value)
    => value = new XyzwStruct { X = 1, Y = 2, Z = 3, W = 4 };

  [Benchmark]
  public void EditingStructViaRef() {
      for (var i = 0; i < n; i++) {
        var value = new XyzwStruct { X = 1, Y = 2, Z = 3, W = 4 };
        this.EditingStructViaRefImpl_(ref value);
      }
    }

  private void EditingStructViaRefImpl_(ref XyzwStruct value) {
      value.X = 1;
      value.Y = 2;
      value.Z = 3;
      value.W = 4;
    }


  [Benchmark]
  public void EditingStructViaInterface() {
      for (var i = 0; i < n; i++) {
        this.EditingStructViaInterfaceImpl_(out var value);
      }
    }

  private void EditingStructViaInterfaceImpl_(out IXyzw value)
    => value = new XyzwStruct { X = 1, Y = 2, Z = 3, W = 4 };
}