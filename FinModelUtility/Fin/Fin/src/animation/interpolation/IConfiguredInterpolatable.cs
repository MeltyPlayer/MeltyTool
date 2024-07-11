using fin.animation.keyframes;
using fin.util.optional;

using schema.readOnly;

namespace fin.animation.interpolation;

public class SharedInterpolationConfig : ISharedInterpolationConfig {
  public int AnimationLength { get; set; }
  public bool Looping { get; set; }
}

public interface ISharedInterpolationConfig {
  int AnimationLength { get; }
  bool Looping { get; }
}

public readonly struct IndividualInterpolationConfig<T>() {
  public int InitialCapacity { get; init; } = 0;
  public IOptional<T>? DefaultValue { get; init; } = null;
}

public interface IConfiguredInterpolatable<T> : IInterpolatable<T> {
  ISharedInterpolationConfig SharedConfig { get; }
  IndividualInterpolationConfig<T> IndividualConfig { get; }
}