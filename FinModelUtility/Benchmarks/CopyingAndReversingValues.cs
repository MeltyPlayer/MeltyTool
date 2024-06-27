using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;

namespace benchmarks {
  public class CopyingAndReversingValues {
    private const int n = 100000000;

    public MemoryStream ms = new MemoryStream(n * sizeof(float));
    public float[] values = new float[n];

    [IterationSetup]
    public void Setup() {
      this.ms.Position = 0;
    }

    [Benchmark]
    public void ReversingWithManualSize() {
      for (var i = 0; i < n; i++) {
        values[i] = ReversingWithManualSizeImpl<float>(sizeof(float));
      }
    }

    public unsafe T ReversingWithManualSizeImpl<T>(int size) {
      Span<byte> span = stackalloc byte[size];
      this.ms.Read(span);

      fixed (byte* ptr = span) {
        span.Reverse();

        return *(T*) ptr;
      }
    }

    [Benchmark]
    public void ReversingWithGenericSize() {
      for (var i = 0; i < n; i++) {
        values[i] = ReversingWithGenericSizeImpl<float>();
      }
    }

    public unsafe T ReversingWithGenericSizeImpl<T>() {
      Span<byte> span = stackalloc byte[sizeof(T)];
      this.ms.Read(span);
      span.Reverse();

      fixed (byte* ptr = span) {
        return *(T*) ptr;
      }
    }


    [Benchmark]
    public void ReversingWithSpans() {
      for (var i = 0; i < n; i++) {
        values[i] = ReversingWithSpansImpl<float>();
      }
    }

    public unsafe T ReversingWithSpansImpl<T>() where T : unmanaged {
      Span<T> tSpan = stackalloc T[1];
      var bSpan = MemoryMarshal.Cast<T, byte>(tSpan);
      this.ms.Read(bSpan);
      bSpan.Reverse();
      return tSpan[0];
    }


    [Benchmark]
    public void ReversingWithPointer() {
      for (var i = 0; i < n; i++) {
        values[i] = ReversingWithSpansImpl<float>();
      }
    }

    public unsafe T ReversingWithPointerImpl<T>() where T : unmanaged {
      T value;
      T* ptr = &value;
      var bSpan = new Span<byte>(ptr, sizeof(T));
      this.ms.Read(bSpan);
      bSpan.Reverse();
      return value;
    }



    [Benchmark]
    public void ReversingDirectly() {
      for (var i = 0; i < n; i++) {
        ReversingDirectlyImpl(out this.values[i]);
      }
    }

    public unsafe void ReversingDirectlyImpl<T>(out T val) where T : unmanaged {
      fixed (T* ptr = &val) {
        var bSpan = new Span<byte>(ptr, sizeof(T));
        this.ms.Read(bSpan);
        bSpan.Reverse();
      }
    }


    /*private const int nEach = 4;

    [Benchmark]
    public void ReversingViaReverse() {
      byte[] bytes = new byte[nEach * n];
      float[] floats = new float[n];

      for (var i = 0; i < n; i++) {
        Array.Reverse(bytes, nEach * i, nEach);
        floats[i] = BitConverter.ToSingle(bytes, i);
      }
    }

    [Benchmark]
    public void ReversingViaSpan() {
      Span<byte> bytes = stackalloc byte[nEach * n];
      float[] floats = new float[n];

      for (var i = 0; i < n; i++) {
        var span = bytes.Slice(nEach * i, nEach);
        span.Reverse();
        floats[i] = BitConverter.ToSingle(span);
      }
    }

    [Benchmark]
    public unsafe void ReversingViaPointer() {
      float[] floats = new float[n];

      fixed (float* ptr = floats) {
        for (var i = 0; i < n; i++) {
          var span = new Span<byte>(ptr + i, nEach);
          span.Reverse();
        }
      }
    }*/
  }
}