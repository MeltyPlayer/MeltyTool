using System;
using System.Collections.Generic;
using System.Numerics;

using fin.language.equations.fixedFunction.impl;

namespace fin.language.equations.fixedFunction;

public interface IFixedFunctionEquations<TIdentifier> {
  IColorOps ColorOps { get; }
  IScalarOps ScalarOps { get; }

  IScalarConstant CreateScalarConstant(double v);

  IColorConstant CreateColorConstant(
      double r,
      double g,
      double b);

  IColorConstant CreateColorConstant(in Vector3 rgb)
    => this.CreateColorConstant(rgb.X, rgb.Y, rgb.Z);


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
public interface IValue;

public interface IConstant : IValue;

public interface ITerm : IValue;

public interface IExpression : IValue;

// Typed
public interface IValue<TValue> : IValue where TValue : IValue<TValue> {
  TValue Add(TValue term1, params TValue[] terms);
  TValue Subtract(TValue term1, params TValue[] terms);
  TValue Multiply(TValue factor1, params TValue?[] factors);
  TValue Divide(TValue factor1, params TValue[] factors);

  TValue Add(IScalarValue term1, params IScalarValue[] terms);
  TValue Subtract(IScalarValue term1, params IScalarValue[] terms);
  TValue Multiply(IScalarValue factor1, params IScalarValue[] factors);
  TValue Divide(IScalarValue factor1, params IScalarValue[] factors);
}

public interface IConstant<TValue> : IConstant, IValue<TValue>
    where TValue : IValue<TValue>;

public interface ITerm<TValue> : ITerm, IValue<TValue>
    where TValue : IValue<TValue> {
  IReadOnlyList<TValue> NumeratorFactors { get; }
  IReadOnlyList<TValue>? DenominatorFactors { get; }
}

public interface IExpression<TValue> : IExpression, IValue<TValue>
    where TValue : IValue<TValue>
 {
  IReadOnlyList<TValue> Terms { get; }
}