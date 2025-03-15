using System.Collections.Generic;
using System.Linq;

using fin.language.equations.fixedFunction.util;
using fin.util.enumerables;

namespace fin.language.equations.fixedFunction;

public abstract class BColorValue : IColorValue {
  public IColorValue? Add(IColorValue? term1,
                          params IEnumerable<IColorValue?> terms) {
    return new ColorExpression(
        this.AsTerms()
            .Concat(term1)
            .Concat(terms.ToArray())
            .WhereNonnull()
            .ToArray());
  }

  public IColorValue? Subtract(IColorValue? term1,
                               params IEnumerable<IColorValue?> terms)
    => this.Add(term1.Negate(), terms.Select(v => v.Negate()));

  public IColorValue? Multiply(IColorValue? factor1,
                               params IEnumerable<IColorValue?> factors) {
    var (numerators, denominators) = this.AsRatio();
    return new ColorTerm(numerators
                         .Concat(factor1)
                         .Concat(factors)
                         .WhereNonnull()
                         .ToArray(),
                         denominators
                             .WhereNonnull()
                             .ToArray());
  }

  public IColorValue? Divide(IColorValue? factor1,
                             params IEnumerable<IColorValue?> factors) {
    var (numerators, denominators) = this.AsRatio();
    return new ColorTerm(numerators
                         .WhereNonnull()
                         .ToArray(),
                         denominators
                             .Concat(factor1)
                             .Concat(factors)
                             .WhereNonnull()
                             .ToArray());
  }

  public IColorValue Add(IScalarValue? term1,
                         params IEnumerable<IScalarValue?> terms)
    => new ColorExpression(
        this.AsTerms()
            .Concat(term1.Yield().Concat(terms).ToColorValues())
            .WhereNonnull()
            .ToArray());

  public IColorValue Subtract(IScalarValue? term1,
                              params IEnumerable<IScalarValue?> terms)
    => this.Add(term1.Negate(), terms.Select(v => v.Negate()));

  public IColorValue Multiply(IScalarValue? factor1,
                              params IEnumerable<IScalarValue?> factors) {
    var (numerators, denominators) = this.AsRatio();
    return new ColorTerm(numerators
                         .Concat(factor1.Yield()
                                        .Concat(factors)
                                        .ToColorValues())
                         .WhereNonnull()
                         .ToArray(),
                         denominators
                             .WhereNonnull()
                             .ToArray());
  }

  public IColorValue Divide(IScalarValue? factor1,
                            params IEnumerable<IScalarValue?> factors) {
    var (numerators, denominators) = this.AsRatio();
    return new ColorTerm(numerators
                         .WhereNonnull()
                         .ToArray(),
                         denominators
                             .Concat(factor1.Yield()
                                            .Concat(factors)
                                            .ToColorValues())
                             .WhereNonnull()
                             .ToArray());
  }

  public abstract IScalarValue? Intensity { get; }
  public abstract IScalarValue R { get; }
  public abstract IScalarValue G { get; }
  public abstract IScalarValue B { get; }

  public bool Clamp { get; set; }
}