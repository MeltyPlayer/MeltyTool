using BenchmarkDotNet.Attributes;

namespace benchmarks {
  public class ReturningClassVsStruct {
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
    public void ReturnClass() {
      for (var i = 0; i < n; i++) {
        var value = this.ReturnClassImpl_();
      }
    }

    private XyzwClass ReturnClassImpl_()
      => new XyzwClass {X = 1, Y = 2, Z = 3, W = 4};


    [Benchmark]
    public void ReturnClassViaInterface() {
      for (var i = 0; i < n; i++) {
        var value = this.ReturnClassViaInterfaceImpl_();
      }
    }

    private IXyzw ReturnClassViaInterfaceImpl_()
      => new XyzwClass { X = 1, Y = 2, Z = 3, W = 4 };


    [Benchmark]
    public void ReturnStruct() {
      for (var i = 0; i < n; i++) {
        var value = this.ReturnStructImpl_();
      }
    }

    private XyzwStruct ReturnStructImpl_() {
      return new XyzwStruct { X = 1, Y = 2, Z = 3, W = 4 };
    }


    [Benchmark]
    public void ReturnStructTwice() {
      for (var i = 0; i < n; i++) {
        var value = this.ReturnStructTwiceImpl1_();
      }
    }

    private XyzwStruct ReturnStructTwiceImpl1_() {
      return ReturnStructTwiceImpl2_();
    }

    private XyzwStruct ReturnStructTwiceImpl2_() {
      return new XyzwStruct { X = 1, Y = 2, Z = 3, W = 4 };
    }


    [Benchmark]
    public void ReturnStructByRefTwice() {
      for (var i = 0; i < n; i++) {
        this.ReturnStructByRefTwiceImpl1_(out var value);
      }
    }

    private void ReturnStructByRefTwiceImpl1_(out XyzwStruct value) {
      value = ReturnStructTwiceImpl2_();
    }

    private void ReturnStructByRefTwiceImpl2_(out XyzwStruct value) {
      value = new XyzwStruct { X = 1, Y = 2, Z = 3, W = 4 };
    }




    [Benchmark]
    public void ReturnStructViaInterface() {
      for (var i = 0; i < n; i++) {
        var value = this.ReturnStructViaInterfaceImpl_();
      }
    }

    private IXyzw ReturnStructViaInterfaceImpl_() {
      return new XyzwStruct { X = 1, Y = 2, Z = 3, W = 4 };
    }
  }
}