using fin.util.optional;

namespace fin.animation.interpolation;

public interface ISharedInterpolationConfig {
  int AnimationLength { get; }
  bool Looping { get; }

  /// <summary>
  ///   Whether to manually disable the fix for rotations where it tries to use
  ///   the nearest rotation value on the circle.
  ///
  ///   For example, the default behavior when rotation from 15 degrees to 345
  ///   degrees is to take the shorter path, i.e. to treat 345 as -15 degrees
  ///   instead.
  ///
  ///   Manually disabling this will force it to use the original rotation
  ///   value. Usually, this results in rendering bugs where bones rotate
  ///   farther than expected. But in some rare cases, such as Super Smash
  ///   Bros. Melee, this needs to be disabled to allow rotations that use big
  ///   deltas to go very fast (i.e. the screw attack animation).
  /// </summary>
  bool DisableNearestRotationFix { get; set; }
}

public class SharedInterpolationConfig : ISharedInterpolationConfig {
  public int AnimationLength { get; set; }
  public bool Looping { get; set; }
  public bool DisableNearestRotationFix { get; set; }
}

public interface IIndividualInterpolationConfig {
  int? AnimationLength { get; }
  int InitialCapacity { get; }
}

public class IndividualInterpolationConfig<T>()
    : IIndividualInterpolationConfig {
  public int? AnimationLength { get; init; }
  public int InitialCapacity { get; init; } = 0;
  public IOptional<T>? DefaultValue { get; init; } = null;
}

public interface IConfiguredInterpolatable<T> : IInterpolatable<T> {
  ISharedInterpolationConfig SharedConfig { get; }
  IndividualInterpolationConfig<T> IndividualConfig { get; }
}