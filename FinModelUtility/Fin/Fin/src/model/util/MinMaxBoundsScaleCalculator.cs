using System.Numerics;


namespace fin.model.util;

public record Bounds(float MinX,
                     float MinY,
                     float MinZ,
                     float MaxX,
                     float MaxY,
                     float MaxZ) {
  public Vector3 Dimensions => new(this.MaxX - this.MinX,
                                   this.MaxY - this.MinY,
                                   this.MaxZ - this.MinZ);
}

public interface IMinMaxBoundsScaleCalculator<in T> {
  Bounds CalculateBounds(T value);
  float CalculateScale(T value);
}

public abstract class BMinMaxBoundsScaleCalculator<T>
    : IMinMaxBoundsScaleCalculator<T> {
  public abstract Bounds CalculateBounds(T value);

  public float CalculateScale(T value)
    => this.ConvertBoundsToScale_(this.CalculateBounds(value));

  private float ConvertBoundsToScale_(Bounds bounds) {
    var scale = 1000 / bounds.Dimensions.Length();
    return float.IsInfinity(scale) ? 1 : scale;
  }
}