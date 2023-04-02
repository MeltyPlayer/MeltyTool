using System;

namespace fin.model.util {
  public record Bounds(float MinX,
                       float MinY,
                       float MinZ,
                       float MaxX,
                       float MaxY,
                       float MaxZ);

  public interface IMinMaxBoundsScaleCalculator<in T> {
    float CalculateScale(T value);
    Bounds CalculateBounds(T value);
  }

  public abstract class BMinMaxBoundsScaleCalculator<T>
      : IMinMaxBoundsScaleCalculator<T> {
    public float CalculateScale(T value) {
      var bounds = CalculateBounds(value);
      return 1000 / MathF.Sqrt(MathF.Pow(bounds.MaxX - bounds.MinX, 2) +
                               MathF.Pow(bounds.MaxY - bounds.MinY, 2) +
                               MathF.Pow(bounds.MaxZ - bounds.MinZ, 2));
    }

    public abstract Bounds CalculateBounds(T value);
  }
}