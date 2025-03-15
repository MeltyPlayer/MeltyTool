using System.Collections.Generic;
using System.Linq;

using schema.util.enumerables;

namespace fin.language.equations.fixedFunction.util;

using UScalarRatio = (IEnumerable<IScalarValue?> numerators, IEnumerable<IScalarValue?> denominators);

public static class ScalarValueExtensions {
  public static IScalarValue? Negate(this IScalarValue? value)
    => value != null
        ? new ScalarTerm([ScalarConstant.NEGATIVE_ONE, value])
        : null;

  public static IEnumerable<IColorValue?> ToColorValues(
      this IEnumerable<IScalarValue?> scalars)
    => scalars.WhereNonnull().Select(scalar => new ColorWrapper(scalar));

  public static IEnumerable<IScalarValue?> AsTerms(this IScalarValue? value)
    => (value as IScalarExpression)?.Terms ?? value.Yield();

  public static UScalarRatio AsRatio(this IScalarValue? value) {
    if (value is IScalarTerm term) {
      return (term.NumeratorFactors, term.DenominatorFactors ?? []);
    }

    return ([value], []);
  }
}