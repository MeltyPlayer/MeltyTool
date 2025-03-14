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
                ((float) colorConstant.IntensityValue).IsRoughly(intensity)) ||
               (((float) colorConstant.RValue).IsRoughly(intensity) &&
                ((float) colorConstant.GValue).IsRoughly(intensity) &&
                ((float) colorConstant.BValue).IsRoughly(intensity)),
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
            => ((float) scalarConstant.Value).IsRoughly(intensity),
        null => intensity.IsRoughly0(),
        _    => false
    };
}