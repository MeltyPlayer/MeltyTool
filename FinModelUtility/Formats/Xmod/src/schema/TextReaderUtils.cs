using System.Numerics;

using schema.text;
using schema.text.reader;

namespace xmod.schema;

public static class TextReaderUtils {
  public static string[] OPEN_BRACE = [" {"];
  public static string[] CLOSING_BRACE = ["{"];
  public static string[] COLON = [":"];
  public static string[] QUOTE = ["\""];

  public static string ReadKeyValue(ITextReader tr, string prefix) {
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      tr.AssertString(prefix);
      tr.AssertChar(':');
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      return tr.ReadLine();
    }

  public static TNumber ReadKeyValueNumber<TNumber>(
      ITextReader tr,
      string prefix)
      where TNumber : INumber<TNumber>
    => TNumber.Parse(ReadKeyValue(tr, prefix), null);

  public static T ReadKeyValueInstance<T>(ITextReader tr, string prefix)
      where T : ITextDeserializable, new() {
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      tr.AssertString(prefix);
      tr.AssertChar(':');
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      var instance = tr.ReadNew<T>();
      tr.SkipOnceIfPresent(TextReaderConstants.NEWLINE_STRINGS);
      return instance;
    }

  public static T[] ReadKeyValueInstances<T>(ITextReader tr,
                                             string prefix,
                                             int count)
      where T : ITextDeserializable, new() {
      var values = new T[count];
      for (var i = 0; i < count; ++i) {
        values[i] = ReadKeyValueInstance<T>(tr, prefix);
      }

      return values;
    }


  public static T[] ReadInstances<T>(ITextReader tr,
                                     string prefix,
                                     int count)
      where T : ITextDeserializable, new() {
      var values = new T[count];
      for (var i = 0; i < count; ++i) {
        tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
        tr.AssertString(prefix);
        tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
        values[i] = tr.ReadNew<T>();
        tr.SkipOnceIfPresent(TextReaderConstants.NEWLINE_STRINGS);
      }

      return values;
    }
}