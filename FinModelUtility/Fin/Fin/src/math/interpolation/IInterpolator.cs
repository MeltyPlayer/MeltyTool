namespace fin.math.interpolation;

public interface IInterpolator<T> : IInterpolator<T, T> { }

public interface IInterpolator<in TIn, out TOut> {
  public TOut Interpolate(TIn fromValue, TIn toValue, float progress);

  public TOut Interpolate(
      float fromTime,
      TIn fromValue,
      float fromTangent,
      float toTime,
      TIn toValue,
      float toTangent,
      float time);
}