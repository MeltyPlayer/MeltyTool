using System.Collections.Generic;
using System.Linq;

using fin.language.equations.fixedFunction.util;
using fin.util.enumerables;

namespace fin.language.equations.fixedFunction;

public abstract class BScalarValue : IScalarValue {
  public IScalarValue Add(IScalarValue term1,
                          params IEnumerable<IScalarValue> terms) {
    var nonZeroTerms = this.AsTerms()
                           .Concat(term1)
                           .Concat(terms.ToArray())
                           .RemoveZeroes()
                           .ToArray();
    if (!nonZeroTerms.Any()) {
      return ScalarConstant.ZERO;
    }

    return new ScalarExpression(nonZeroTerms);
  }

  public IScalarValue Subtract(IScalarValue term1,
                               params IEnumerable<IScalarValue> terms)
    => this.Add(term1.Negate(), terms.Select(v => v.Negate()));

  public IScalarValue Multiply(IScalarValue factor1,
                               params IEnumerable<IScalarValue> factors)
    => this.MultiplyImpl_(factor1.Yield().Concat(factors), []);

  public IScalarValue Divide(IScalarValue factor1,
                             params IEnumerable<IScalarValue> factors)
    => this.MultiplyImpl_([], factor1.Yield().Concat(factors));

  private IScalarValue MultiplyImpl_(
      IEnumerable<IScalarValue> otherNumerators,
      IEnumerable<IScalarValue> otherDenominators) {
    var (myNumerators, myDenominators) = this.AsRatio();

    var nonOneNumerators
        = myNumerators.Concat(otherNumerators).RemoveOnes().ToArray();
    var nonOneDenominators
        = myDenominators.Concat(otherDenominators).RemoveOnes().ToArray();

    if (nonOneNumerators.Length == 0 && nonOneDenominators.Length == 0) {
      return ScalarConstant.ONE;
    }

    if (nonOneNumerators.Any(v => v.IsZero())) {
      return ScalarConstant.ZERO;
    }

    if (nonOneNumerators.Length == 0) {
      nonOneNumerators = ScalarConstant.ONE_ARRAY;
    }

    if (nonOneDenominators.Length == 0) {
      nonOneDenominators = null;
    }

    return new ScalarTerm(nonOneNumerators, nonOneDenominators);
  }

  public bool Clamp { get; set; }

  public IColorValueTernaryOperator TernaryOperator(
      BoolComparisonType comparisonType,
      IScalarValue other,
      IColorValue trueValue,
      IColorValue falseValue)
    => new ColorValueTernaryOperator {
        ComparisonType = comparisonType,
        Lhs = this,
        Rhs = other,
        TrueValue = trueValue,
        FalseValue = falseValue
    };
}