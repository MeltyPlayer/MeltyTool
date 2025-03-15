using System.Collections.Generic;
using System.Linq;

using fin.language.equations.fixedFunction.util;
using fin.util.enumerables;

namespace fin.language.equations.fixedFunction;

public abstract class BColorValue : IColorValue {
  public IColorValue Add(IColorValue term1,
                         params IEnumerable<IColorValue> terms) {
    var nonZeroTerms = this.AsTerms()
                           .Concat(term1)
                           .Concat(terms.ToArray())
                           .RemoveZeroes()
                           .ToArray();
    if (!nonZeroTerms.Any()) {
      return ColorConstant.ZERO;
    }

    return new ColorExpression(nonZeroTerms);
  }

  public IColorValue Subtract(IColorValue term1,
                              params IEnumerable<IColorValue> terms)
    => this.Add(term1.Negate(), terms.Select(v => v.Negate()));

  public IColorValue Multiply(IColorValue factor1,
                              params IEnumerable<IColorValue> factors)
    => this.MultiplyImpl_(factor1.Yield().Concat(factors), []);

  public IColorValue Divide(IColorValue factor1,
                            params IEnumerable<IColorValue> factors)
    => this.MultiplyImpl_([], factor1.Yield().Concat(factors));

  private IColorValue MultiplyImpl_(
      IEnumerable<IColorValue> otherNumerators,
      IEnumerable<IColorValue> otherDenominators) {
    var (myNumerators, myDenominators) = this.AsRatio();

    var nonOneNumerators
        = myNumerators.Concat(otherNumerators).RemoveOnes().ToArray();
    var nonOneDenominators
        = myDenominators.Concat(otherDenominators).RemoveOnes().ToArray();

    if (nonOneNumerators.Length == 0 && nonOneDenominators.Length == 0) {
      return ColorConstant.ONE;
    }

    if (nonOneNumerators.Any(v => v.IsZero())) {
      return ColorConstant.ZERO;
    }

    if (nonOneNumerators.Length == 0) {
      nonOneNumerators = ColorConstant.ONE_ARRAY;
    }

    if (nonOneDenominators.Length == 0) {
      nonOneDenominators = null;
    }

    return new ColorTerm(nonOneNumerators, nonOneDenominators);
  }

  public IColorValue Add(IScalarValue term1,
                         params IEnumerable<IScalarValue> terms)
    => this.Add(ColorConstant.ZERO,
                term1.Yield().Concat(terms).ToColorValues());

  public IColorValue Subtract(IScalarValue term1,
                              params IEnumerable<IScalarValue> terms)
    => this.Add(term1.Negate(), terms.Select(v => v.Negate()));

  public IColorValue Multiply(IScalarValue factor1,
                              params IEnumerable<IScalarValue> factors)
    => this.MultiplyImpl_(factor1.Yield().Concat(factors).ToColorValues(),
                          []);

  public IColorValue Divide(IScalarValue factor1,
                            params IEnumerable<IScalarValue> factors)
    => this.MultiplyImpl_([],
                          factor1.Yield().Concat(factors).ToColorValues());


  public abstract IScalarValue? Intensity { get; }
  public abstract IScalarValue R { get; }
  public abstract IScalarValue G { get; }
  public abstract IScalarValue B { get; }

  public bool Clamp { get; set; }
}