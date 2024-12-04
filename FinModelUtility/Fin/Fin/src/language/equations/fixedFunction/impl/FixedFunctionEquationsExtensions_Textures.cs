using fin.model;

namespace fin.language.equations.fixedFunction;

public static partial class FixedFunctionEquationsExtensions {
  public static bool DoOutputsDependOnTextureSource(
      this IFixedFunctionEquations<FixedFunctionSource> impl,
      int i) => impl.DoOutputsDependOn(
  [
      FixedFunctionSource.TEXTURE_COLOR_0 + i,
      FixedFunctionSource.TEXTURE_ALPHA_0 + i
  ]);
}