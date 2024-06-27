using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace fin.util.asserts;

public static class Expect {
  public static void Fail(string? message = null)
    => Assert.Fail(message);

  public static void IsTrue(bool condition, string? message = null)
    => Assert.IsTrue(condition, message);

  public static void AreEqual<T>(T expected, T actual, string? message = null)
    => Assert.AreEqual(expected, actual, message);

  public static void AreArraysEqual<T>(
      IEnumerable<T> expected,
      IEnumerable<T> actual,
      string? message = null) {
    var differences = new List<string>();

    var expectedArray = expected.ToArray();
    var actualArray = actual.ToArray();

    var expectedCount = expectedArray.Length;
    var actualCount = actualArray.Length;
    if (expectedCount != actualCount) {
      differences.Add(
          $"Expected length to be {expectedCount} but was actually {actualCount}.");
    }

    for (var i = 0; i < Math.Max(expectedCount, actualCount); ++i) {
      var validExpected = i < expectedCount;
      var validActual = i < actualCount;

      if (validExpected &&
          validActual &&
          (expectedArray[i]?.Equals(actualArray[i]) ?? false)) {
        continue;
      }

      var expectedValue = validExpected ? $"{expectedArray[i]}" : "n/a";
      var actualValue = validActual ? $"{actualArray[i]}" : "n/a";

      differences.Add($"  [{i}]: '{expectedValue}' != '{actualValue}'");
    }

    if (differences.Count <= 0) {
      return;
    }

    var allDifferences = new StringBuilder();

    if (message != null) {
      allDifferences.AppendLine(message).AppendLine();
    }

    foreach (var difference in differences) {
      allDifferences.AppendLine(difference);
    }
    Assert.Fail(allDifferences.ToString());
  }
}