using System.Collections.Generic;
using System.Linq;

using fin.language.equations.fixedFunction.util;
using fin.util.enumerables;

namespace fin.language.equations.fixedFunction;

public abstract class BScalarValue : IScalarValue {
  public IScalarValue Add(IScalarValue term1,
                          params IEnumerable<IScalarValue> terms)
    => new ScalarExpression(
        this.AsTerms()
            .Concat(term1)
            .Concat(terms.ToArray())
            .ToArray());

  public IScalarValue Subtract(IScalarValue term1,
                               params IEnumerable<IScalarValue> terms)
    => this.Add(term1.Negate(), terms.Select(v => v.Negate()));

  public IScalarValue Multiply(IScalarValue factor1,
                               params IEnumerable<IScalarValue> factors) {
    var (numerators, denominators) = this.AsRatio();
    return new ScalarTerm(numerators
                          .Concat(factor1)
                          .Concat(factors)
                          .ToArray(),
                          denominators
                              .ToArray());
  }

  public IScalarValue Divide(IScalarValue factor1,
                             params IEnumerable<IScalarValue> factors) {
    var (numerators, denominators) = this.AsRatio();
    return new ScalarTerm(numerators
                              .ToArray(),
                          denominators
                              .Concat(factor1)
                              .Concat(factors)
                              .ToArray());
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