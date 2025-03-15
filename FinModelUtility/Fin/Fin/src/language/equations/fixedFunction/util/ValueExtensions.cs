using System;

using fin.math.floats;

namespace fin.language.equations.fixedFunction.util;

public static class ValueExtensions {
  public static bool IsZero(this IValue? value)
    => value switch {
        IColorValue colorValue => colorValue.IsRoughly(0),
        IScalarValue scalarValue => scalarValue.IsRoughly(0),
        null => true,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

  public static bool IsOne(this IValue? value)
    => value switch {
        IColorValue colorValue => colorValue.IsRoughly(1),
        IScalarValue scalarValue => scalarValue.IsRoughly(1),
        null => false,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

  public static bool IsRoughly(this IColorValue? value, float intensity)
    => value switch {
        IColorConstant colorConstant
            => (colorConstant.IntensityValue != null &&
                colorConstant.IntensityValue.Value.IsRoughly_(intensity)) ||
               (colorConstant.RValue.IsRoughly_(intensity) &&
                colorConstant.GValue.IsRoughly_(intensity) &&
                colorConstant.BValue.IsRoughly_(intensity)),
        ColorWrapper colorWrapper
            => (colorWrapper.Intensity != null &&
                colorWrapper.Intensity.IsRoughly(intensity)) ||
               (colorWrapper.R.IsRoughly(intensity) &&
                colorWrapper.G.IsRoughly(intensity) &&
                colorWrapper.B.IsRoughly(intensity)),
        null => intensity.IsRoughly0(),
        _    => false
    };

  public static bool IsRoughly(this IScalarValue? value, float intensity)
    => value switch {
        IScalarConstant scalarConstant
            => scalarConstant.Value.IsRoughly_(intensity),
        null => intensity.IsRoughly0(),
        _    => false
    };

  public const float TOLERANCE = 1 / 255f;

  private static bool IsRoughly_(this double actual, float expected)
    => Math.Abs(actual - expected) < TOLERANCE;
}