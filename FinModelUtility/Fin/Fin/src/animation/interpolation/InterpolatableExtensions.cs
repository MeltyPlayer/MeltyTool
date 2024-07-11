using System.Runtime.CompilerServices;

namespace fin.animation.interpolation;

public static class InterpolatableExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryGetAtFrameOrDefault<T>(
      this IConfiguredInterpolatable<T> impl,
      float frame,
      out T value)
    => impl.TryGetAtFrameOrDefault(frame, impl.IndividualConfig, out value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryGetAtFrameOrDefault<T>(
      this IInterpolatable<T> impl,
      float frame,
      IndividualInterpolationConfig<T> individualConfig,
      out T value) {
    if (impl.TryGetAtFrame(frame, out value)) {
      return true;
    }

    if (individualConfig.DefaultValue?.Try(out value) ?? false) {
      return true;
    }

    value = default;
    return false;
  }
}