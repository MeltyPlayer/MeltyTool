using System.Collections.Generic;
using System.Linq;

using fin.language.equations.fixedFunction.impl;

using schema.util.enumerables;

namespace fin.language.equations.fixedFunction.util;

using UScalarRatio = (IEnumerable<IScalarValue> numerators,
    IEnumerable<IScalarValue>denominators);

public static class ScalarValueExtensions {
  public static IEnumerable<IScalarValue> RemoveZeroes(
      this IEnumerable<IScalarValue> values)
    => values.Where(v => !FixedFunctionConstants.SIMPLIFY || !v.IsZero());

  public static IEnumerable<IScalarValue> RemoveOnes(
      this IEnumerable<IScalarValue> values)
    => values.Where(v => !v.IsOne());

  public static IScalarValue Negate(this IScalarValue value)
    => new ScalarTerm([ScalarConstant.NEGATIVE_ONE, value]);

  public static IEnumerable<IColorValue> ToColorValues(
      this IEnumerable<IScalarValue> scalars)
    => scalars.WhereNonnull().Select(scalar => new ColorWrapper(scalar));

  public static IEnumerable<IScalarValue> AsTerms(this IScalarValue value)
    => (value as IScalarExpression)?.Terms ?? value.Yield();

  public static UScalarRatio AsRatio(this IScalarValue value) {
    if (value is IScalarTerm term) {
      return (term.NumeratorFactors, term.DenominatorFactors ?? []);
    }

    return ([value], []);
  }

  public static bool IsConstant(this IScalarValue? value,
                                out double constantValue) {
    if (value is IScalarConstant scalarConstant) {
      constantValue = scalarConstant.Value;
      return true;
    }

    constantValue = default;
    return false;
  }
}