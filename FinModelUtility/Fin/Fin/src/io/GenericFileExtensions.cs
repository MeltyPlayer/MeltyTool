using System;
using System.IO;
using System.Runtime.CompilerServices;

using fin.util.json;

using schema.binary;
using schema.text;
using schema.text.reader;

namespace fin.io {
  public static class GenericFileExtensions {
    // JSON Serialization
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Deserialize<T>(this IReadOnlyGenericFile file)
      => JsonUtil.Deserialize<T>(file.ReadAllText());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<T>(this IGenericFile file, T instance)
        where T : notnull
      => file.WriteAllText(JsonUtil.Serialize(instance));


    // Read methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StreamReader OpenReadAsText(this IReadOnlyGenericFile file)
      => new(file.OpenRead());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadNew<T>(this IReadOnlyGenericFile file)
        where T : IBinaryDeserializable, new() {
      using var br = new SchemaBinaryReader(file.OpenRead());
      return br.ReadNew<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadNew<T>(this IReadOnlyGenericFile file,
                               Endianness endianness)
        where T : IBinaryDeserializable, new() {
      using var br = new SchemaBinaryReader(file.OpenRead(), endianness);
      return br.ReadNew<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadNewFromText<T>(this IReadOnlyGenericFile file)
        where T : ITextDeserializable, new() {
      using var tr = new SchemaTextReader(file.OpenRead());
      return tr.ReadNew<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ReadAllBytes(this IReadOnlyGenericFile file) {
      using var s = file.OpenRead();
      using var ms = new MemoryStream();
      s.CopyTo(ms);
      return ms.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadAllText(this IReadOnlyGenericFile file) {
      using var sr = file.OpenReadAsText();
      return sr.ReadToEnd();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] ReadAllLines(this IReadOnlyGenericFile file)
      => file.ReadAllText()
             .Split(TextReaderConstants.NEWLINE_STRINGS,
                    StringSplitOptions.None);


    // Write methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StreamWriter OpenWriteAsText(this IGenericFile file)
      => new(file.OpenWrite());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAllBytes(this IGenericFile file,
                                     ReadOnlyMemory<byte> bytes) {
      using var s = file.OpenWrite();
      s.Write(bytes.Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAllText(this IGenericFile file,
                                    string text) {
      using var sw = file.OpenWriteAsText();
      sw.Write(text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(this IGenericFile file, T data)
        where T : IBinarySerializable, new() {
      using var fs = file.OpenWrite();
      using var bw = new SchemaBinaryWriter();
      data.Write(bw);
      bw.CompleteAndCopyTo(fs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(this IGenericFile file,
                                T data,
                                Endianness endianness)
        where T : IBinarySerializable, new() {
      using var fs = file.OpenWrite();
      using var bw = new SchemaBinaryWriter(endianness);
      data.Write(bw);
      bw.CompleteAndCopyTo(fs);
    }
  }
}