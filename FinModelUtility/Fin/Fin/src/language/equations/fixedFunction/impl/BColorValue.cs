using System;
using System.Collections.ObjectModel;
using System.Linq;

using fin.util.lists;

using NoAlloq;

namespace fin.language.equations.fixedFunction;

public abstract class BColorValue : IColorValue {
  public IColorValue Add(IColorValue term1,
                         params ReadOnlySpan<IColorValue> terms)
    => new ColorExpression(ListUtil.ReadonlyFrom(this, term1, terms));

  public IColorValue Subtract(IColorValue term1,
                              params ReadOnlySpan<IColorValue> terms)
    => new ColorExpression(
        ListUtil.ReadonlyFrom(
            this,
            (ReadOnlySpan<IColorValue>) this.NegateTerms(term1, terms)));

  public IColorValue Multiply(IColorValue factor1,
                              params ReadOnlySpan<IColorValue> factors)
    => new ColorTerm(ListUtil.ReadonlyFrom(this, factor1, factors));

  public IColorValue Divide(IColorValue factor1,
                            params ReadOnlySpan<IColorValue> factors)
    => new ColorTerm(ListUtil.ReadonlyFrom(this),
                     ListUtil.ReadonlyFrom(factor1, factors));

  protected IColorValue[] NegateTerms(IColorValue term1,
                                      params ReadOnlySpan<IColorValue> terms)
    => this.NegateTerms(ListUtil.From(term1, terms).ToArray());

  protected IColorValue[] NegateTerms(params ReadOnlySpan<IColorValue> terms)
    => terms.Select(
                term => new ColorTerm(
                    ListUtil.ReadonlyFrom(
                        ColorConstant.NEGATIVE_ONE,
                        term)))
            .ToArray();


  public IColorValue Add(IScalarValue term1,
                         params ReadOnlySpan<IScalarValue> terms)
    => new ColorExpression(
        ListUtil.ReadonlyFrom(this,
                              (ReadOnlySpan<IColorValue>) this.ToColorValues(
                                  term1,
                                  terms)));

  public IColorValue Subtract(IScalarValue term1,
                              params ReadOnlySpan<IScalarValue> terms)
    => new ColorExpression(
        ListUtil.ReadonlyFrom(
            this,
            (ReadOnlySpan<IColorValue>) this.ToColorValues(
                this.NegateTerms(term1, terms))));

  public IColorValue Multiply(IScalarValue factor1,
                              params ReadOnlySpan<IScalarValue> factors)
    => new ColorTerm(ListUtil.ReadonlyFrom(
                         this,
                         (ReadOnlySpan<IColorValue>) this.ToColorValues(
                             factor1,
                             factors)));

  public IColorValue Divide(IScalarValue factor1,
                            params ReadOnlySpan<IScalarValue> factors)
    => new ColorTerm(ListUtil.ReadonlyFrom(this),
                     new ReadOnlyCollection<IColorValue>(
                         this.ToColorValues(factor1, factors)));

  protected IScalarValue[] NegateTerms(IScalarValue term1,
                                       params ReadOnlySpan<IScalarValue> terms)
    => this.NegateTerms(ListUtil.From(term1, terms).ToArray());

  protected IScalarValue[] NegateTerms(params ReadOnlySpan<IScalarValue> terms)
    => terms.Select(
                term => new ScalarTerm([
                    ScalarConstant.NEGATIVE_ONE,
                    term
                ]))
            .ToArray();

  protected IColorValue[] ToColorValues(
      params ReadOnlySpan<IScalarValue> scalars)
    => scalars.Select(scalar => new ColorWrapper(scalar)).ToArray();

  protected IColorValue[] ToColorValues(
      IScalarValue first,
      params ReadOnlySpan<IScalarValue> scalars)
    => this.ToColorValues(ListUtil.From(first, scalars).ToArray());


  public abstract IScalarValue? Intensity { get; }
  public abstract IScalarValue R { get; }
  public abstract IScalarValue G { get; }
  public abstract IScalarValue B { get; }

  public bool Clamp { get; set; }
}