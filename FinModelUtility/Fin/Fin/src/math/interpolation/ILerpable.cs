namespace fin.math.interpolation;

public interface ILerpable<T> where T : ILerpable<T> {
  static abstract T Lerp(T lhs, T rhs, float progress);
}