using System.Linq;

using fin.language.equations.fixedFunction;
using fin.util.lists;

namespace fin.src.language.equations.fixedFunction.impl;
public abstract class BScalarValue : IScalarValue {
  public virtual IScalarValue Add(
      IScalarValue term1,
      params IScalarValue[] terms)
    => new ScalarExpression(ListUtil.ReadonlyFrom(this, term1, terms));

  public virtual IScalarValue Subtract(
      IScalarValue term1,
      params IScalarValue[] terms)
    => new ScalarExpression(
        ListUtil.ReadonlyFrom(
            this,
            this.NegateTerms(term1, terms)));

  public virtual IScalarValue Multiply(
      IScalarValue factor1,
      params IScalarValue[] factors)
    => new ScalarTerm(ListUtil.ReadonlyFrom(this, factor1, factors));

  public virtual IScalarValue Divide(
      IScalarValue factor1,
      params IScalarValue[] factors)
    => new ScalarTerm(ListUtil.ReadonlyFrom(this),
                      ListUtil.ReadonlyFrom(factor1, factors));

  protected IScalarValue[] NegateTerms(
      IScalarValue term1,
      params IScalarValue[] terms)
    => this.NegateTerms(ListUtil.From(term1, terms).ToArray());

  protected IScalarValue[] NegateTerms(
      params IScalarValue[] terms)
    => terms.Select(
                term => new ScalarTerm(
                    ListUtil.ReadonlyFrom(
                        ScalarConstant.NEGATIVE_ONE,
                        term)))
            .ToArray();

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