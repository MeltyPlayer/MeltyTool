namespace fin.language.equations.fixedFunction;

public interface IScalarNamedValue : INamedValue, IScalarFactor {
  IScalarValue ScalarValue { get; }
}

public interface IScalarIdentifiedValue<out TIdentifier> :
    IIdentifiedValue<TIdentifier>,
    IScalarFactor;

public interface IScalarInput<out TIdentifier>
    : IScalarIdentifiedValue<TIdentifier>;

public interface IScalarOutput<out TIdentifier>
    : IScalarIdentifiedValue<TIdentifier> {
  IScalarValue ScalarValue { get; }
}


public interface IScalarValue
    : IValue<IScalarValue, IScalarConstant, IScalarTerm, IScalarExpression> {
  bool Clamp { get; set; }

  IColorValueTernaryOperator TernaryOperator(
      BoolComparisonType comparisonType,
      IScalarValue other,
      IColorValue trueValue,
      IColorValue falseValue);
}

public interface IScalarTerm
    : IScalarValue,
      ITerm<IScalarValue, IScalarConstant, IScalarTerm, IScalarExpression> { }

public interface IScalarExpression
    : IScalarValue,
      IExpression<IScalarValue, IScalarConstant, IScalarTerm,
          IScalarExpression> { }


public interface IScalarFactor : IScalarValue { }

public interface IScalarConstant
    : IScalarFactor,
      IConstant<IScalarValue, IScalarConstant, IScalarTerm,
          IScalarExpression> {
  double Value { get; }
}