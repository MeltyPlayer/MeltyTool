using fin.language.equations.fixedFunction.impl;

namespace fin.language.equations.fixedFunction;

public static class FixedFunctionOpsExtensions {
  public static TValue AddOrSubtractOp<TValue, TConstant>(
      this IFixedFunctionOps<TValue, TConstant> ops,
      bool isAdd,
      TValue a,
      TValue b,
      TValue c,
      TValue d,
      IScalarValue bias,
      IScalarValue scale)
      where TValue : IValue<TValue>
      where TConstant : IConstant<TValue>, TValue {
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