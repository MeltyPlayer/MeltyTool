using System;
using System.Linq;
using System.Collections.Generic;

using fin.data.queues;
using fin.util.linq;

using Newtonsoft.Json.Linq;

namespace fin.language.equations.fixedFunction;

public partial class FixedFunctionEquations<TIdentifier>
    : IFixedFunctionEquations<TIdentifier> {
  public bool HasInput(TIdentifier identifier)
    => this.ColorInputs.ContainsKey(identifier) ||
       this.ScalarInputs.ContainsKey(identifier);

  public bool DoOutputsDependOn(TIdentifier[] outputIdentifiers,
                                IValue value)
    => this.DoOutputsDependOn_(outputIdentifiers, value.Equals);

  public bool DoOutputsDependOn(TIdentifier[] outputIdentifiers,
                                TIdentifier identifier)
    => this.DoOutputsDependOn_(
        outputIdentifiers,
        value => value is IIdentifiedValue<TIdentifier> identifiedValue &&
                 identifier.Equals(identifiedValue.Identifier));

  public bool DoOutputsDependOn(TIdentifier[] outputIdentifiers,
                                TIdentifier[] identifiers) {
    var identifierSet = identifiers.ToHashSet();
    return this.DoOutputsDependOn_(
        outputIdentifiers,
        value => value is IIdentifiedValue<TIdentifier> identifiedValue &&
                 identifierSet.Contains(identifiedValue.Identifier));
  }

  private bool DoOutputsDependOn_(
      TIdentifier[] outputIdentifiers,
      Func<IValue, bool> checker) {
    var colorQueue = new Queue<IColorValue>();
    var scalarQueue = new Queue<IScalarValue>();

    foreach (var outputIdentifier in outputIdentifiers) {
      if (this.colorOutputs_.TryGetValue(outputIdentifier,
                                         out var colorOutput)) {
        colorQueue.Enqueue(colorOutput);
      }

      if (this.scalarOutputs_.TryGetValue(outputIdentifier,
                                          out var scalarOutput)) {
        scalarQueue.Enqueue(scalarOutput);
      }
    }

    bool didUpdate;
    do {
      didUpdate = false;
      if (colorQueue.TryDequeue(out var colorValue)) {
        didUpdate = true;

        if (checker(colorValue)) {
          return true;
        }

        switch (colorValue) {
          case IColorConstant:
          case IColorInput<TIdentifier>: {
            break;
          }
          case IColorOutput<TIdentifier> colorIdentifiedValue: {
            colorQueue.Enqueue(colorIdentifiedValue.ColorValue);
            break;
          }
          case IColorNamedValue colorNamedValue: {
            colorQueue.Enqueue(colorNamedValue.ColorValue);
            break;
          }
          case IColorExpression colorExpression: {
            foreach (var term in colorExpression.Terms) {
              colorQueue.Enqueue(term);
            }

            break;
          }
          case IColorTerm colorTerm: {
            foreach (var numerator in colorTerm.NumeratorFactors) {
              colorQueue.Enqueue(numerator);
            }

            if (colorTerm.DenominatorFactors != null) {
              foreach (var denominator in colorTerm.DenominatorFactors) {
                colorQueue.Enqueue(denominator);
              }
            }

            break;
          }
          case IColorValueTernaryOperator colorValueTernaryOperator: {
            scalarQueue.Enqueue(colorValueTernaryOperator.Lhs);
            scalarQueue.Enqueue(colorValueTernaryOperator.Rhs);
            colorQueue.Enqueue(colorValueTernaryOperator.TrueValue);
            colorQueue.Enqueue(colorValueTernaryOperator.FalseValue);
            break;
          }
          default: {
            if (colorValue.Intensity != null) {
              scalarQueue.Enqueue(colorValue.Intensity);
            } else {
              scalarQueue.Enqueue(colorValue.R);
              scalarQueue.Enqueue(colorValue.G);
              scalarQueue.Enqueue(colorValue.B);
            }

            break;
          }
        }
      }

      if (scalarQueue.TryDequeue(out var scalarValue)) {
        didUpdate = true;

        if (checker(scalarValue)) {
          return true;
        }

        switch (scalarValue) {
          case IScalarConstant:
          case IScalarInput<TIdentifier>: {
            break;
          }
          case IScalarOutput<TIdentifier> scalarIdentifiedValue: {
            scalarQueue.Enqueue(scalarIdentifiedValue.ScalarValue);
            break;
          }
          case IScalarNamedValue scalarNamedValue: {
            scalarQueue.Enqueue(scalarNamedValue.ScalarValue);
            break;
          }
          case IColorValueSwizzle colorValueSwizzle: {
            colorQueue.Enqueue(colorValueSwizzle.Source);
            break;
          }
          case IColorNamedValueSwizzle<TIdentifier> colorNamedValueSwizzle: {
            colorQueue.Enqueue(colorNamedValueSwizzle.Source);
            break;
          }
          case IScalarExpression scalarExpression: {
            foreach (var term in scalarExpression.Terms) {
              scalarQueue.Enqueue(term);
            }

            break;
          }
          case IScalarTerm scalarTerm: {
            foreach (var numerator in scalarTerm.NumeratorFactors) {
              scalarQueue.Enqueue(numerator);
            }

            if (scalarTerm.DenominatorFactors != null) {
              foreach (var denominator in scalarTerm.DenominatorFactors) {
                scalarQueue.Enqueue(denominator);
              }
            }

            break;
          }
          default: {
            break;
          }
        }
      }
    } while (didUpdate);

    return false;
  }
}