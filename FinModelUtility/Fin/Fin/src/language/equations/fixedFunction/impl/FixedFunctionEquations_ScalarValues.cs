using System;
using System.Collections.Generic;
using System.Linq;

using fin.util.asserts;
using fin.util.lists;

namespace fin.language.equations.fixedFunction;

// TODO: Optimize this.
public partial class FixedFunctionEquations<TIdentifier> {
  private readonly Dictionary<double, IScalarConstant> scalarConstants_ =
      new();

  private readonly Dictionary<TIdentifier, IScalarInput<TIdentifier>>
      scalarInputs_ = new();

  private readonly Dictionary<TIdentifier, IScalarOutput<TIdentifier>>
      scalarOutputs_ = new();

  public IReadOnlyDictionary<TIdentifier, IScalarInput<TIdentifier>>
      ScalarInputs => this.scalarInputs_;

  public IReadOnlyDictionary<TIdentifier, IScalarOutput<TIdentifier>>
      ScalarOutputs => this.scalarOutputs_;

  public IScalarConstant CreateScalarConstant(double v) {
    if (this.scalarConstants_.TryGetValue(
            v,
            out var scalarConstant)) {
      return scalarConstant;
    }

    return this.scalarConstants_[v] = new ScalarConstant(v);
  }

  public IScalarInput<TIdentifier> CreateOrGetScalarInput(
      TIdentifier identifier) {
    Asserts.False(this.scalarOutputs_.ContainsKey(identifier));

    if (!this.scalarInputs_.TryGetValue(identifier, out var input)) {
      input = new ScalarInput(identifier);
      this.scalarInputs_[identifier] = input;
    }

    return input;
  }

  public IScalarOutput<TIdentifier> CreateScalarOutput(
      TIdentifier identifier,
      IScalarValue value) {
    Asserts.False(this.scalarInputs_.ContainsKey(identifier));
    Asserts.False(this.scalarOutputs_.ContainsKey(identifier));

    var output = new ScalarOutput(identifier, value);
    this.scalarOutputs_[identifier] = output;
    return output;
  }


  private class ScalarInput(TIdentifier identifier)
      : BScalarValue, IScalarInput<TIdentifier> {
    public TIdentifier Identifier { get; } = identifier;
  }

  private class ScalarOutput(TIdentifier identifier, IScalarValue value)
      : BScalarValue, IScalarOutput<TIdentifier> {
    public TIdentifier Identifier { get; } = identifier;
    public IScalarValue ScalarValue { get; } = value;
  }
}


public abstract class BScalarValue : IScalarValue {
  public virtual IScalarExpression Add(
      IScalarValue term1,
      params IScalarValue[] terms)
    => new ScalarExpression(ListUtil.ReadonlyFrom(this, term1, terms));

  public virtual IScalarExpression Subtract(
      IScalarValue term1,
      params IScalarValue[] terms)
    => new ScalarExpression(
        ListUtil.ReadonlyFrom(
            this,
            this.NegateTerms(term1, terms)));

  public virtual IScalarTerm Multiply(
      IScalarValue factor1,
      params IScalarValue[] factors)
    => new ScalarTerm(ListUtil.ReadonlyFrom(this, factor1, factors));

  public virtual IScalarTerm Divide(
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

public class ScalarExpression : BScalarValue, IScalarExpression {
  public ScalarExpression(IReadOnlyList<IScalarValue> terms) {
    this.Terms = terms;
  }

  public IReadOnlyList<IScalarValue> Terms { get; }

  public override IScalarExpression Add(
      IScalarValue term1,
      params IScalarValue[] terms)
    => new ScalarExpression(
        ListUtil.ReadonlyConcat(this.Terms, new[] { term1 }, terms));

  public override IScalarExpression Subtract(
      IScalarValue term1,
      params IScalarValue[] terms)
    => new ScalarExpression(
        ListUtil.ReadonlyConcat(this.Terms,
                                this.NegateTerms(term1, terms)));
}

public class ScalarTerm : BScalarValue, IScalarTerm {
  public ScalarTerm(
      IReadOnlyList<IScalarValue> numeratorFactors,
      IReadOnlyList<IScalarValue>? denominatorFactors = null) {
    this.NumeratorFactors = numeratorFactors;
    this.DenominatorFactors = denominatorFactors;
  }

  public IReadOnlyList<IScalarValue> NumeratorFactors { get; }
  public IReadOnlyList<IScalarValue>? DenominatorFactors { get; }

  public override IScalarTerm Multiply(
      IScalarValue factor1,
      params IScalarValue[] factors)
    => new ScalarTerm(ListUtil.ReadonlyConcat(
                          this.NumeratorFactors,
                          ListUtil.ReadonlyFrom(factor1, factors)));

  public override IScalarTerm Divide(
      IScalarValue factor1,
      params IScalarValue[] factors)
    => new ScalarTerm(this.NumeratorFactors,
                      ListUtil.ReadonlyConcat(
                          this.DenominatorFactors,
                          ListUtil.ReadonlyFrom(factor1, factors)));
}

public class ScalarConstant : BScalarValue, IScalarConstant {
  public static readonly ScalarConstant ONE = new(1);
  public static readonly ScalarConstant NEGATIVE_ONE = new(-1);

  public ScalarConstant(double value) {
    this.Value = value;
  }

  public double Value { get; }

  public override string ToString() => $"{this.Value}";


  public override bool Equals(object? other) {
    if (Object.ReferenceEquals(this, other)) {
      return true;
    }

    if (other is IScalarConstant otherScalar) {
      return FixedFunctionUtils.CompareScalarConstants(
          this.Value,
          otherScalar.Value);
    }

    if (other is IColorConstant otherColor) {
      return FixedFunctionUtils.CompareScalarConstants(
          this.Value,
          otherColor.IntensityValue);
    }

    if (other is ColorWrapper {
            Intensity: IScalarConstant colorWrapperIntensity
        }) {
      return FixedFunctionUtils.CompareScalarConstants(
          this.Value,
          colorWrapperIntensity.Value);
    }

    return false;
  }
}