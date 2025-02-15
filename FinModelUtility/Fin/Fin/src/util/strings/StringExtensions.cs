using System;
using System.Text;
using System.Text.RegularExpressions;

using fin.util.asserts;

namespace fin.util.strings;

public static class StringExtensions {
  public static string Reverse(this string str) {
    var sb = new StringBuilder();
    for (var i = str.Length - 1; i >= 0; --i) {
      sb.Append(str[i]);
    }

    return sb.ToString();
  }

  public static bool TryRemoveStart(
      this string str,
      string start,
      out string trimmed) {
    if (str.StartsWith(start)) {
      trimmed = str.Substring(start.Length);
      return true;
    }

    trimmed = default;
    return false;
  }

  public static string AssertRemoveStart(this string str, string start) {
    Asserts.True(str.TryRemoveStart(start, out var trimmed));
    return trimmed;
  }

  public static (string, string) SplitBeforeAndAfterFirst(
      this string text,
      char separator) {
    var index = text.IndexOf(separator);
    Asserts.True(index >= 0);
    return (text[..index], text[(index + 1)..]);
  }

  public static string[] SplitNewlines(this string text)
    => Regex.Split(text, "\r\n|\r|\n");

  public static string SubstringUpTo(this string str, char c) {
    var indexTo = str.IndexOf(c);
    return indexTo >= 0 ? str[..indexTo] : str;
  }

  public static ReadOnlySpan<char> SubstringUpTo(
      this ReadOnlySpan<char> str,
      char c) {
    var indexTo = str.IndexOf(c);
    return indexTo >= 0 ? str[..indexTo] : str;
  }

  public static ReadOnlySpan<char> SubstringUpTo(
      this ReadOnlySpan<char> str,
      ReadOnlySpan<char> s) {
    var indexTo = str.IndexOf(s);
    return indexTo >= 0 ? str[..indexTo] : str;
  }



  public static string SubstringUpTo(this string str, string substr) {
    var indexTo = str.IndexOf(substr);
    return indexTo >= 0 ? str[..indexTo] : str;
  }

  public static string SubstringAfter(this string str, string substr) {
    var indexTo = str.IndexOf(substr);
    return indexTo >= 0 ? str[(indexTo + substr.Length)..] : str;
  }

  public static string Repeat(this string str, int times) {
    if (times == 0) {
      return "";
    }

    if (times == 1) {
      return str;
    }

    var builder = new StringBuilder();
    for (var i = 0; i < times; ++i) {
      builder.Append(str);
    }

    return builder.ToString();
  }
}