using fin.model;

namespace fin.language.equations.fixedFunction;

public static partial class FixedFunctionEquationsExtensions {
  public static FixedFunctionSource[] OUTPUT_IDENTIFIERS = [
      FixedFunctionSource.OUTPUT_COLOR,
      FixedFunctionSource.OUTPUT_ALPHA
  ];

  public static bool DoOutputsDependOn(
      this IFixedFunctionEquations<FixedFunctionSource> equations,
      IValue value)
    => equations.DoOutputsDependOn(OUTPUT_IDENTIFIERS, value);

  public static bool DoOutputsDependOn(
      this IFixedFunctionEquations<FixedFunctionSource> equations,
      FixedFunctionSource identifier)
    => equations.DoOutputsDependOn(OUTPUT_IDENTIFIERS, identifier);

  public static bool DoOutputsDependOn(
      this IFixedFunctionEquations<FixedFunctionSource> equations,
      FixedFunctionSource[] identifiers)
    => equations.DoOutputsDependOn(OUTPUT_IDENTIFIERS, identifiers);
}