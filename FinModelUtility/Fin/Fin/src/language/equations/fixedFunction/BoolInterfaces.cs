namespace fin.language.equations.fixedFunction;

public enum BoolComparisonType {
  EQUAL_TO,
  NOT_EQUAL_TO,
  GREATER_THAN,
  GREATER_THAN_OR_EQUAL_TO,
  LESS_THAN,
  LESS_THAN_OR_EQUAL_TO,
}

public interface ITernaryOperator<out TOut> {
  BoolComparisonType ComparisonType { get; }
  IScalarValue Lhs { get; }
  IScalarValue Rhs { get; }

  TOut TrueValue { get; }
  TOut FalseValue { get; }
}

public interface IColorValueTernaryOperator :
    ITernaryOperator<IColorValue>,
    IColorValue;