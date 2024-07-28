using System;

namespace UoT.util {
  public class AssertException : Exception {
    public AssertException() : base() { }

    public AssertException(string message) : base(message) { }

    public AssertException(string message, Exception innerException) :
        base(message, innerException) { }
  }

  public static class Asserts {
    public static void Fail(string? message = null) {
      if (message != null) {
        throw new AssertException(message);
      } else {
        throw new AssertException();
      }
    }

    public static T Assert<T>(T? value, string? message = null)
        where T : class {
      if (value == null) {
        Fail(message);
      }
      return value!;
    }
  }
}