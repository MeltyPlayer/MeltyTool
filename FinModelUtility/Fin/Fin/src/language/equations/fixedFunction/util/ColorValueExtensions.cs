using System.Collections.Generic;
using System.Linq;

using schema.util.enumerables;

namespace fin.language.equations.fixedFunction.util;

using UColorRatio = (IEnumerable<IColorValue> numerators, IEnumerable<IColorValue> denominators);

public static class ColorValueExtensions {
  public static IEnumerable<IColorValue> RemoveZeroes(
      this IEnumerable<IColorValue> values)
    => values.Where(v => !v.IsZero());

  public static IEnumerable<IColorValue> RemoveOnes(
      this IEnumerable<IColorValue> values)
    => values.Where(v => !v.IsOne());

  public static IColorValue Negate(this IColorValue value)
    => new ColorTerm([ColorConstant.NEGATIVE_ONE, value]);

  public static IEnumerable<IColorValue> AsTerms(this IColorValue value)
    => (value as IColorExpression)?.Terms ?? value.Yield();

  public static UColorRatio AsRatio(this IColorValue value) {
    if (value is IColorTerm term) {
      return (term.NumeratorFactors, term.DenominatorFactors ?? []);
    }

    return ([value], []);
  }
}