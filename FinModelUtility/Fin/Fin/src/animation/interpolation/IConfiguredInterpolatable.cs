using fin.util.optional;

namespace fin.animation.interpolation;

public interface ISharedInterpolationConfig {
  int AnimationLength { get; }
  bool Looping { get; }
}

public class SharedInterpolationConfig : ISharedInterpolationConfig {
  public int AnimationLength { get; set; }
  public bool Looping { get; set; }
}

public interface IIndividualInterpolationConfig {
  int? AnimationLength { get; }
  int InitialCapacity { get; }
}

public readonly struct IndividualInterpolationConfig<T>()
    : IIndividualInterpolationConfig {
  public int? AnimationLength { get; init; }
  public int InitialCapacity { get; init; } = 0;
  public IOptional<T>? DefaultValue { get; init; } = null;
}

public interface IConfiguredInterpolatable<T> : IInterpolatable<T> {
  ISharedInterpolationConfig SharedConfig { get; }
  IndividualInterpolationConfig<T> IndividualConfig { get; }
}