namespace fin.language.equations.fixedFunction;

public interface IColorNamedValue : INamedValue, IColorFactor {
  IColorValue ColorValue { get; }
}

public interface IColorIdentifiedValue<out TIdentifier>
    : IIdentifiedValue<TIdentifier>,
      IColorFactor;

public interface IColorInput<out TIdentifier>
    : IColorIdentifiedValue<TIdentifier>;

public interface IColorOutput<out TIdentifier>
    : IColorIdentifiedValue<TIdentifier> {
  IColorValue ColorValue { get; }
}

public enum ColorSwizzle {
  R,
  G,
  B,
}

public interface IColorNamedValueSwizzle<out TIdentifier> : IScalarFactor {
  IColorIdentifiedValue<TIdentifier> Source { get; }
  ColorSwizzle SwizzleType { get; }
}

public interface IColorValueSwizzle : IScalarFactor {
  IColorValue Source { get; }
  ColorSwizzle SwizzleType { get; }
}

public interface IColorValue : IValue<IColorValue> {
  IScalarValue? Intensity { get; }
  IScalarValue R { get; }
  IScalarValue G { get; }
  IScalarValue B { get; }

  bool Clamp { get; set; }
}

public interface IColorTerm : IColorValue, ITerm<IColorValue>;

public interface IColorExpression : IColorValue, IExpression<IColorValue>;

public interface IColorFactor : IColorValue;

public interface IColorConstant : IColorFactor, IConstant<IColorValue> {
  double? IntensityValue { get; }
  double RValue { get; }
  double GValue { get; }
  double BValue { get; }
}