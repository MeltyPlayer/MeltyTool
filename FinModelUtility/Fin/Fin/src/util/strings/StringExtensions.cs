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
      trimmed = str[start.Length..];
      return true;
    }

    trimmed = default;
    return false;
  }

  public static string AssertRemoveStart(this string str, string start) {
    Asserts.True(str.TryRemoveStart(start, out var trimmed));
    return trimmed;
  }

  public static bool TryRemoveEnd(
      this string str,
      string end,
      out string trimmed) {
    if (str.EndsWith(end)) {
      trimmed = str[..^end.Length];
      return true;
    }

    trimmed = default;
    return false;
  }

  public static string ReplaceFirst(this string str,
                                    char target,
                                    char replacement) {
    var index = str.IndexOf(target);
    if (index < 0) {
      return str;
    }

    var sb = new StringBuilder(str.Length);
    sb.Append(str[..index]);
    sb.Append(replacement);
    sb.Append(str[(index + 1)..]);
    return sb.ToString();
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
}