using System;
using System.Collections.Generic;

using fin.language.equations.fixedFunction.impl;

namespace fin.language.equations.fixedFunction;

public partial class FixedFunctionEquations<TIdentifier>
    : IFixedFunctionEquations<TIdentifier> where TIdentifier : notnull {
  private readonly HashSet<IValue> valueDependencies_ = new();
  private readonly HashSet<TIdentifier> identifierDependencies_ = new();

  public IColorOps ColorOps { get; }
  public IScalarOps ScalarOps { get; }

  public FixedFunctionEquations() {
    this.ColorOps = new ColorFixedFunctionOps<TIdentifier>(this);
    this.ScalarOps = new ScalarFixedFunctionOps<TIdentifier>(this);
  }

  public bool HasInput(TIdentifier identifier)
    => this.ColorInputs.ContainsKey(identifier) ||
       this.ScalarInputs.ContainsKey(identifier);

  public bool DoOutputsDependOn(IValue value)
    => this.valueDependencies_.Contains(value);

  public bool DoOutputsDependOn(ReadOnlySpan<TIdentifier> identifiers) {
    foreach (var identifier in identifiers) {
      if (this.DoOutputsDependOn(identifier)) {
        return true;
      }
    }

    return false;
  }

  public bool DoOutputsDependOn(TIdentifier identifier)
    => this.identifierDependencies_.Contains(identifier);

  private void AddValueDependency_(IColorValue colorValue) {
    if (!this.valueDependencies_.Add(colorValue)) {
      return;
    }

    switch (colorValue) {
      case IColorConstant: {
        break;
      }
      case IColorInput<TIdentifier> colorIdentifiedInput: {
        this.identifierDependencies_.Add(colorIdentifiedInput.Identifier);
        break;
      }
      case IColorOutput<TIdentifier> colorIdentifiedOutput: {
        this.identifierDependencies_.Add(colorIdentifiedOutput.Identifier);
        this.AddValueDependency_(colorIdentifiedOutput.ColorValue);
        break;
      }
      case IColorNamedValue colorNamedValue: {
        this.AddValueDependency_(colorNamedValue.ColorValue);
        break;
      }
      case IColorExpression colorExpression: {
        foreach (var term in colorExpression.Terms) {
          this.AddValueDependency_(term);
        }

        break;
      }
      case IColorTerm colorTerm: {
        foreach (var numerator in colorTerm.NumeratorFactors) {
          this.AddValueDependency_(numerator);
        }

        if (colorTerm.DenominatorFactors != null) {
          foreach (var denominator in colorTerm.DenominatorFactors) {
            this.AddValueDependency_(denominator);
          }
        }

        break;
      }
      case IColorValueTernaryOperator colorValueTernaryOperator: {
        this.AddValueDependency_(colorValueTernaryOperator.Lhs);
        this.AddValueDependency_(colorValueTernaryOperator.Rhs);
        this.AddValueDependency_(colorValueTernaryOperator.TrueValue);
        this.AddValueDependency_(colorValueTernaryOperator.FalseValue);
        break;
      }
      default: {
        if (colorValue.Intensity != null) {
          this.AddValueDependency_(colorValue.Intensity);
        } else {
          this.AddValueDependency_(colorValue.R);
          this.AddValueDependency_(colorValue.G);
          this.AddValueDependency_(colorValue.B);
        }

        break;
      }
    }
  }

  private void AddValueDependency_(IScalarValue scalarValue) {
    if (!this.valueDependencies_.Add(scalarValue)) {
      return;
    }

    switch (scalarValue) {
      case IScalarConstant: {
        break;
      }
      case IScalarInput<TIdentifier> scalarIdentifiedInput: {
        this.identifierDependencies_.Add(scalarIdentifiedInput.Identifier);
        break;
      }
      case IScalarOutput<TIdentifier> scalarIdentifiedOutput: {
        this.identifierDependencies_.Add(scalarIdentifiedOutput.Identifier);
        this.AddValueDependency_(scalarIdentifiedOutput.ScalarValue);
        break;
      }
      case IScalarNamedValue scalarNamedValue: {
        this.AddValueDependency_(scalarNamedValue.ScalarValue);
        break;
      }
      case IColorValueSwizzle colorValueSwizzle: {
        this.AddValueDependency_(colorValueSwizzle.Source);
        break;
      }
      case IColorNamedValueSwizzle<TIdentifier> colorNamedValueSwizzle: {
        this.AddValueDependency_(colorNamedValueSwizzle.Source);
        break;
      }
      case IScalarExpression scalarExpression: {
        foreach (var term in scalarExpression.Terms) {
          this.AddValueDependency_(term);
        }

        break;
      }
      case IScalarTerm scalarTerm: {
        foreach (var numerator in scalarTerm.NumeratorFactors) {
          this.AddValueDependency_(numerator);
        }

        if (scalarTerm.DenominatorFactors != null) {
          foreach (var denominator in scalarTerm.DenominatorFactors) {
            this.AddValueDependency_(denominator);
          }
        }

        break;
      }
    }
  }
}