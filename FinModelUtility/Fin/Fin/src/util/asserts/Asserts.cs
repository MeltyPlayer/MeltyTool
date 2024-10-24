using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using fin.math.floats;

namespace fin.util.asserts;

/**
 * NOTE: Using $"" to define messages allocates strings, and can be expensive!
 * Try to avoid allocating strings unless an assertion actually fails.
 */
public class AssertionException(string message) : Exception(message) {
  public override string StackTrace {
    get {
      List<string> stackTrace = [];
      stackTrace.AddRange(base.StackTrace!.Split(Environment.NewLine));

      var assertLine = new Regex("\\s*Asserts\\.");
      stackTrace.RemoveAll(x => assertLine.IsMatch(x));

      return string.Join(Environment.NewLine, stackTrace.ToArray());
    }
  }
}

public class Asserts {
  public static bool Fail(string? message = null)
    => throw new AssertionException(message ?? "Failed.");

  public static bool True(bool value, string? message = null)
    => value || Fail(message ?? "Expected to be true.");

  public static bool False(bool value, string? message = null)
    => True(!value, message ?? "Expected to be false.");

  public static bool Same(
      object instanceA,
      object instanceB,
      string message = "Expected references to be the same.")
    => True(ReferenceEquals(instanceA, instanceB), message);

  public static void Different(
      object instanceA,
      object instanceB,
      string message = "Expected references to be different.") {
    False(ReferenceEquals(instanceA, instanceB), message);
  }

  public static bool Equal(
      object? expected,
      object? actual,
      string? message = null) {
    if (expected?.Equals(actual) ?? false) {
      return true;
    }

    Fail(message ?? $"Expected {actual} to equal {expected}.");
    return false;
  }

  public static void SequenceEqual<TEnumerable>(
      TEnumerable enumerableA,
      TEnumerable enumerableB) where TEnumerable : IEnumerable {
    var enumeratorA = enumerableA.GetEnumerator();
    var enumeratorB = enumerableB.GetEnumerator();

    var hasA = enumeratorA.MoveNext();
    var hasB = enumeratorB.MoveNext();

    var index = 0;
    while (hasA && hasB) {
      var currentA = enumeratorA.Current;
      var currentB = enumeratorB.Current;

      if (!Equals(currentA, currentB)) {
        Fail(
            $"Expected {currentA} to equal {currentB} at index {index}.");
      }

      index++;

      hasA = enumeratorA.MoveNext();
      hasB = enumeratorB.MoveNext();
    }

    True(!hasA && !hasB,
         "Expected enumerables to be the same length.");
  }

  public static void SpansEqual<T>(ReadOnlySpan<T> expected,
                                   ReadOnlySpan<T> actual) {
    var sb = new StringBuilder();
    if (expected.Length != actual.Length) {
      sb.AppendLine($"Expected length {actual.Length} to be {expected.Length}");
    }

    for (var i = 0; i < expected.Length; ++i) {
      if (!expected[i].Equals(actual[i])) {
        sb.AppendLine($"- Expected [{i}] = {actual[i]} to be {expected[i]}");
      }
    }

    if (sb.Length > 0) {
      Fail(sb.ToString());
    }
  }

  public static bool Equal<T>(
      T expected,
      T actual,
      string? message = null) {
    if (expected?.Equals(actual) ?? false) {
      return true;
    }

    Fail(message ?? $"Expected {actual} to equal {expected}.");
    return false;
  }

  public static bool Equal(
      string expected,
      string actual,
      string? message = null)
    => Equal<string>(expected, actual, message);

  public static bool IsRoughly(float expected,
                               float actual,
                               string? message = null) {
    if (expected.IsRoughly(actual)) {
      return true;
    }

    Fail(message ??
         $"Expected {actual} to roughly equal {expected}.");
    return false;
  }

  // Null Checks
  public static bool Nonnull(
      object? instance,
      string? message = null)
    => True(instance != null,
            message ?? "Expected reference to be nonnull.");

  public static T CastNonnull<T>(
      T? instance,
      string? message = null) {
    True(instance != null,
         message ?? "Expected reference to be nonnull.");
    return instance!;
  }

  public static void Null(
      object? instance,
      string message = "Expected reference to be null.")
    => True(instance == null, message);

  // Type checks
  public static bool IsA<TExpected>(object? instance, string? message = null)
    => IsA(instance, typeof(TExpected), message);

  public static bool IsA(
      object? instance,
      Type expected,
      string? message = null)
    => Nonnull(instance, message) &&
       Equal(instance!.GetType(), expected, message);

  public static TExpected AsA<TExpected>(object? instance,
                                         string? message = null) {
    var cast = (TExpected) instance;
    Nonnull(cast, message);
    return cast!;
  }

  public static TSub AsSubType<TBase, TSub>(
      TBase instance,
      string? message = null)
      where TSub : TBase {
    var cast = (TSub) instance;

    if (instance == null &&
        Nullable.GetUnderlyingType(typeof(TSub)) != null) {
      return cast!;
    }

    return CastNonnull(cast, message);
  }
}