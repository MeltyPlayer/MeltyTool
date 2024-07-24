using System;
using System.Collections.Generic;

namespace fin.language.equations.fixedFunction;

public interface IFixedFunctionEquations<TIdentifier> {
  IScalarConstant CreateScalarConstant(double v);

  IColorConstant CreateColorConstant(
      double r,
      double g,
      double b);

  IColorConstant CreateColorConstant(
      double intensity);

  IColorFactor CreateColor(IScalarValue r,
                           IScalarValue g,
                           IScalarValue b);

  IColorFactor CreateColor(IScalarValue intensity);


  IReadOnlyDictionary<TIdentifier, IScalarInput<TIdentifier>>
      ScalarInputs { get; }

  IScalarInput<TIdentifier> CreateOrGetScalarInput(
      TIdentifier identifier);


  IReadOnlyDictionary<TIdentifier, IScalarOutput<TIdentifier>>
      ScalarOutputs { get; }

  IScalarOutput<TIdentifier> CreateScalarOutput(
      TIdentifier identifier,
      IScalarValue value);


  IReadOnlyDictionary<TIdentifier, IColorInput<TIdentifier>>
      ColorInputs { get; }

  IColorInput<TIdentifier> CreateOrGetColorInput(
      TIdentifier identifier);


  IReadOnlyDictionary<TIdentifier, IColorOutput<TIdentifier>>
      ColorOutputs { get; }

  IColorOutput<TIdentifier> CreateColorOutput(
      TIdentifier identifier,
      IColorValue value);

  bool HasInput(TIdentifier identifier);

  bool DoOutputsDependOn(IValue value);
  bool DoOutputsDependOn(TIdentifier identifiers);
  bool DoOutputsDependOn(ReadOnlySpan<TIdentifier> identifiers);
}

public interface IIdentifiedValue<out TIdentifier> : IValue {
  TIdentifier Identifier { get; }
}

public interface INamedValue : IValue {
  string Name { get; }
}

// Simple 
public interface IValue { }

public interface IConstant : IValue { }

public interface ITerm : IValue { }

public interface IExpression : IValue { }

// Typed
public interface IValue<in TValue, TConstant, out TTerm, out TExpression>
    : IValue
    where TValue : IValue<TValue, TConstant, TTerm, TExpression>
    where TConstant : IConstant<TValue, TConstant, TTerm, TExpression>, TValue
    where TTerm : ITerm<TValue, TConstant, TTerm, TExpression>, TValue
    where TExpression : IExpression<TValue, TConstant, TTerm, TExpression>,
    TValue {
  TExpression Add(TValue term1, params TValue[] terms);
  TExpression Subtract(TValue term1, params TValue[] terms);
  TTerm Multiply(TValue factor1, params TValue[] factors);
  TTerm Divide(TValue factor1, params TValue[] factors);

  TExpression Add(IScalarValue term1, params IScalarValue[] terms);
  TExpression Subtract(IScalarValue term1, params IScalarValue[] terms);
  TTerm Multiply(IScalarValue factor1, params IScalarValue[] factors);
  TTerm Divide(IScalarValue factor1, params IScalarValue[] factors);
}

public interface IConstant<in TValue, TConstant, out TTerm, out TExpression>
    : IConstant, IValue<TValue, TConstant, TTerm, TExpression>
    where TValue : IValue<TValue, TConstant, TTerm, TExpression>
    where TConstant : IConstant<TValue, TConstant, TTerm, TExpression>, TValue
    where TTerm : ITerm<TValue, TConstant, TTerm, TExpression>, TValue
    where TExpression : IExpression<TValue, TConstant, TTerm, TExpression>,
    TValue;

public interface ITerm<TValue, TConstant, out TTerm, out TExpression>
    : ITerm, IValue<TValue, TConstant, TTerm, TExpression>
    where TValue : IValue<TValue, TConstant, TTerm, TExpression>
    where TConstant : IConstant<TValue, TConstant, TTerm, TExpression>, TValue
    where TTerm : ITerm<TValue, TConstant, TTerm, TExpression>, TValue
    where TExpression : IExpression<TValue, TConstant, TTerm, TExpression>,
    TValue {
  IReadOnlyList<TValue> NumeratorFactors { get; }
  IReadOnlyList<TValue>? DenominatorFactors { get; }
}

public interface IExpression<TValue, TConstant, out TTerm, out TExpression>
    : IExpression, IValue<TValue, TConstant, TTerm, TExpression>
    where TValue : IValue<TValue, TConstant, TTerm, TExpression>
    where TConstant : IConstant<TValue, TConstant, TTerm, TExpression>, TValue
    where TTerm : ITerm<TValue, TConstant, TTerm, TExpression>, TValue
    where TExpression : IExpression<TValue, TConstant, TTerm, TExpression>,
    TValue {
  IReadOnlyList<TValue> Terms { get; }
}