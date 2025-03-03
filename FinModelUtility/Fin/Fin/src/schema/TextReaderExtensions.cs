using schema.text.reader;

namespace fin.schema;

public static class TextReaderExtensions {
  public static void SkipWhitespace(this ITextReader tr)
    => tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);

  public static string ReadWord(this ITextReader tr) {
    tr.SkipWhitespace();
    return tr.ReadUpToStartOfTerminator([
        " ", "\t", "\n", "\r\n", ",", "{", "[", "}", "]", ":"
    ]);
  }
}