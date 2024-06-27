using System.Numerics;

using BenchmarkDotNet.Attributes;

using fin.math.rotations;

using MathNet.Numerics;

namespace benchmarks;

public class QuaternionCreation {
  private const int n = 1000;

  private const float X_RADIANS = 0;
  private const float Y_RADIANS = 0;
  private const float Z_RADIANS = 0;





  [GlobalSetup]
  public void Setup() {
      FinTrig.Cos(0);
    }


  [Benchmark]
  public void CheckViaIfs() {
      for (var i = 0; i < n; i++) {
        var q = Quaternion.Identity;

        if (!X_RADIANS.AlmostEqual(0, .001)) {
          q *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, X_RADIANS);
        }

        if (!Y_RADIANS.AlmostEqual(0, .001)) {
          q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, Y_RADIANS);
        }

        if (!Z_RADIANS.AlmostEqual(0, .001)) {
          q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, Z_RADIANS);
        }

        q = Quaternion.Normalize(q);
      }
    }

  [Benchmark]
  public void NoChecks() {
      for (var i = 0; i < n; i++) {
        var q = Quaternion.Identity;

        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, X_RADIANS);
        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, Y_RADIANS);
        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, Z_RADIANS);

        q = Quaternion.Normalize(q);
      }
    }

  [Benchmark]
  public void NoTemp() {
      for (var i = 0; i < n; i++) {
        var q = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, X_RADIANS) *
             Quaternion.CreateFromAxisAngle(Vector3.UnitY, Y_RADIANS) *
             Quaternion.CreateFromAxisAngle(Vector3.UnitX, Z_RADIANS));
      }
    }

  [Benchmark]
  public void Manually() {
      for (var i = 0; i < n; i++) {
        var cr = FinTrig.Cos(X_RADIANS * 0.5f);
        var sr = FinTrig.Sin(X_RADIANS * 0.5f);
        var cp = FinTrig.Cos(Y_RADIANS * 0.5f);
        var sp = FinTrig.Sin(Y_RADIANS * 0.5f);
        var cy = FinTrig.Cos(Z_RADIANS * 0.5f);
        var sy = FinTrig.Sin(Z_RADIANS * 0.5f);

        var q = new Quaternion(
            sr * cp * cy - cr * sp * sy,
            cr * sp * cy + sr * cp * sy,
            cr * cp * sy - sr * sp * cy,
            cr * cp * cy + sr * sp * sy);
      }
    }
}