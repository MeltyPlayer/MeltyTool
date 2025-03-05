namespace fin.math.interpolation;

public interface IInterpolator<T> : IInterpolator<T, T>;

public interface IInterpolator<TIn, out TOut> {
  public TOut Interpolate(in TIn fromValue, in TIn toValue, float progress);

  public TOut Interpolate(
      float fromTime,
      in TIn fromValue,
      float fromTangent,
      float toTime,
      in TIn toValue,
      float toTangent,
      float time);
}