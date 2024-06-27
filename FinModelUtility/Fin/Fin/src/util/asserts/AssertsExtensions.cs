namespace fin.util.asserts;

public static class AssertsExtensions {
  public static TExpected AssertAsA<TExpected>(
      this object? instance,
      string? message = null)
    => Asserts.AsA<TExpected>(instance, message);
}