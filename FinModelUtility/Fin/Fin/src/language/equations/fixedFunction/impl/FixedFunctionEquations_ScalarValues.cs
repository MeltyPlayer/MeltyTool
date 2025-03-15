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

    this.AddValueDependency_(value);

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

public class ScalarExpression(IReadOnlyList<IScalarValue> terms)
    : BScalarValue, IScalarExpression {
  public IReadOnlyList<IScalarValue> Terms { get; } = terms;
}

public class ScalarTerm(
    IReadOnlyList<IScalarValue> numeratorFactors,
    IReadOnlyList<IScalarValue>? denominatorFactors = null)
    : BScalarValue, IScalarTerm {
  public IReadOnlyList<IScalarValue> NumeratorFactors { get; }
    = numeratorFactors;

  public IReadOnlyList<IScalarValue>? DenominatorFactors { get; }
    = denominatorFactors;
}

public class ScalarConstant(double value) : BScalarValue, IScalarConstant {
  public static readonly ScalarConstant ONE = new(1);
  public static readonly ScalarConstant NEGATIVE_ONE = new(-1);

  public double Value { get; } = value;

  public override string ToString() => $"{this.Value}";


  public override bool Equals(object? other) {
    if (ReferenceEquals(this, other)) {
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