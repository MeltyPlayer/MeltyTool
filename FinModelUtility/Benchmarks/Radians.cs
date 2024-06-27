using BenchmarkDotNet.Attributes;

using fin.math.rotations;

namespace benchmarks {
  public class Radians {
    private const int n = 100000;

    public const float lhs = 1;
    public const float rhs = 2;

    public const float PI2 = MathF.PI * 2;
    public const float PI3 = MathF.PI * 3;


    [Benchmark]
    public void Inline() {
      for (var i = 0; i < n; i++) {
        var radians = ((((lhs - rhs) % PI2) + PI3) % PI2) - MathF.PI;
      }
    }

    [Benchmark]
    public void StaticCall() {
      for (var i = 0; i < n; i++) {
        var radians =
            RadiansUtil.CalculateRadiansTowards(Radians.lhs, Radians.rhs);
      }
    }

    private readonly ReadonlyRadiansFloat lhsStruct = new(1);
    private readonly ReadonlyRadiansFloat rhsStruct = new(2);

    [Benchmark]
    public void ReadonlyStruct() {
      for (var i = 0; i < n; i++) {
        var radians = lhsStruct - rhsStruct;
      }
    }


    [Benchmark]
    public void ReadonlyStructEach() {
      for (var i = 0; i < n; i++) {
        var lhs = new ReadonlyRadiansFloat(1);
        var rhs = new ReadonlyRadiansFloat(2);

        var radians = lhs - rhs;
      }
    }

    [Benchmark]
    public void StructEach() {
      for (var i = 0; i < n; i++) {
        var lhs = new RadiansFloat(1);
        var rhs = new RadiansFloat(2);

        var radians = lhs - rhs;
      }
    }


    public readonly struct ReadonlyRadiansFloat {
      private const float PI2 = MathF.PI * 2;
      private const float PI3 = MathF.PI * 3;

      private readonly float radians_;

      public ReadonlyRadiansFloat(float radians) {
        this.radians_ = (((radians % PI2) + PI3) % PI2) - MathF.PI;
      }

      public ReadonlyRadiansFloat Add(ReadonlyRadiansFloat other)
        => new(this.radians_ + other.radians_);

      public ReadonlyRadiansFloat Subtract(ReadonlyRadiansFloat other)
        => new(this.radians_ - other.radians_);

      public static ReadonlyRadiansFloat operator -(ReadonlyRadiansFloat value)
        => new(-value.radians_);

      public static ReadonlyRadiansFloat operator +(ReadonlyRadiansFloat lhs, ReadonlyRadiansFloat rhs)
        => lhs.Add(rhs);

      public static ReadonlyRadiansFloat operator -(ReadonlyRadiansFloat lhs, ReadonlyRadiansFloat rhs)
        => lhs.Subtract(rhs);
    }

    public struct RadiansFloat {
      private const float PI2 = MathF.PI * 2;
      private const float PI3 = MathF.PI * 3;

      private readonly float radians_;

      public RadiansFloat(float radians) {
        this.radians_ = (((radians % PI2) + PI3) % PI2) - MathF.PI;
      }

      public RadiansFloat Add(RadiansFloat other)
        => new(this.radians_ + other.radians_);

      public RadiansFloat Subtract(RadiansFloat other)
        => new(this.radians_ - other.radians_);

      public static RadiansFloat operator -(RadiansFloat value)
        => new(-value.radians_);

      public static RadiansFloat operator +(RadiansFloat lhs, RadiansFloat rhs)
        => lhs.Add(rhs);

      public static RadiansFloat operator -(RadiansFloat lhs, RadiansFloat rhs)
        => lhs.Subtract(rhs);
    }
  }
}