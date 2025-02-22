using fin.language.equations.fixedFunction.impl;

namespace fin.language.equations.fixedFunction;

public static class FixedFunctionOpsExtensions {
  public static TValue? AddOrSubtractOp<TValue, TConstant, TTerm, TExpression>(
      this IFixedFunctionOps<TValue, TConstant, TTerm, TExpression> ops,
      bool isAdd,
      TValue? a,
      TValue? b,
      TValue? c,
      TValue? d,
      IScalarValue? bias,
      IScalarValue? scale)
      where TValue : IValue<TValue, TConstant, TTerm, TExpression>
      where TConstant : IConstant<TValue, TConstant, TTerm, TExpression>, TValue
      where TTerm : ITerm<TValue, TConstant, TTerm, TExpression>, TValue
      where TExpression : IExpression<TValue, TConstant, TTerm, TExpression>,
      TValue {
    var aTimesOneMinusC = ops.Multiply(
        a,
        ops.Subtract(ops.One, c));

    var bTimesC = ops.Multiply(b, c);

    var rest = ops.Add(aTimesOneMinusC, bTimesC);

    var value = isAdd
        ? ops.Add(d, rest)
        : ops.Subtract(d, rest);

    value = ops.AddWithScalar(value, bias);
    value = ops.MultiplyWithScalar(value, scale);

    return value;
  }
}