using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

using BenchmarkDotNet.Attributes;

using CommunityToolkit.HighPerformance;


namespace benchmarks {
  public class Simd {
    private readonly int n_ = 100;

    private readonly byte[] values_ = Enumerable.Range(0, 100000)
                                                .Select(i => i * 1f)
                                                .ToArray()
                                                .AsSpan()
                                                .AsBytes()
                                                .ToArray();

    [Benchmark]
    public unsafe void IsFinite_Span_Separate() {
      var memory = this.values_.AsMemory();
      var pin = memory.Pin();

      Span<Vector4> elementSpan = stackalloc Vector4[1];
      Span<float> floatSpan = elementSpan.Cast<Vector4, float>();

      var ptr = (byte*) pin.Pointer;
      for (var i = 0; i < this.n_; ++i) {
        for (var v = 0; v < memory.Length; v += 4) {
          var basePtr = (float*) (ptr + v);
          for (var e = 0; e < 4; ++e) {
            floatSpan[e] = basePtr[e];
          }

          CheckIsFiniteSeparate(elementSpan[0]);
        }
      }
    }

    [Benchmark]
    public unsafe void IsFinite_Pointer_Separate() {
      var memory = this.values_.AsMemory();
      var pin = memory.Pin();

      var ptr = (byte*) pin.Pointer;
      for (var i = 0; i < this.n_; ++i) {
        for (var v = 0; v < memory.Length; v += 4) {
          var vec = *(Vector4*) (ptr + v);
          CheckIsFiniteSeparate(vec);
        }
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CheckIsFiniteSeparate(in Vector4 v)
      => float.IsFinite(v.X) &&
         float.IsFinite(v.Y) &&
         float.IsFinite(v.Z) &&
         float.IsFinite(v.W);

    [Benchmark]
    public unsafe void IsFinite_Span_Simd() {
      var memory = this.values_.AsMemory();
      var pin = memory.Pin();

      Span<Vector128<int>> elementSpan = stackalloc Vector128<int>[1];
      Span<float> floatSpan = elementSpan.Cast<Vector128<int>, float>();

      var ptr = (byte*) pin.Pointer;
      for (var i = 0; i < this.n_; ++i) {
        for (var v = 0; v < memory.Length; v += 4) {
          var basePtr = (float*) (ptr + v);
          for (var e = 0; e < 4; ++e) {
            floatSpan[e] = basePtr[e];
          }

          CheckIsFiniteSimd(elementSpan[0]);
        }
      }
    }

    [Benchmark]
    public unsafe void IsFinite_Pointer_Simd() {
      var memory = this.values_.AsMemory();
      var pin = memory.Pin();

      var ptr = (byte*) pin.Pointer;
      for (var i = 0; i < this.n_; ++i) {
        for (var v = 0; v < memory.Length; v += 4) {
          var vec = *(Vector128<int>*) (ptr + v);
          CheckIsFiniteSimd(vec);
        }
      }
    }

    public static readonly Vector128<int> IS_FINITE_MASK = Vector128.CreateScalar(0x7fffffff);
    public static readonly Vector128<int> NAN_OR_INFINITY = Vector128.CreateScalar(0x7F800000);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CheckIsFiniteSimd(in Vector128<int> v)
      => Vector128.GreaterThanOrEqualAny(Vector128.BitwiseAnd(v, IS_FINITE_MASK), NAN_OR_INFINITY);
  }
}