using System.Linq;

using schema.text.reader;

namespace fin.schema;

public static class TextReaderConstantsExtra {
  public static string[] WHITESPACE_STRINGS
      = TextReaderConstants.WHITESPACE_CHARS
                           .Select(c => c.ToString())
                           .ToArray();
}