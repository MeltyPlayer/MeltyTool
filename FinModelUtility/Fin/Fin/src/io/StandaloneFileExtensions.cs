using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using fin.util.json;

using schema.binary;
using schema.text;
using schema.text.reader;

namespace fin.io;

public static class StandaloneFileExtensions {
  // JSON Serialization
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Deserialize<T>(this IReadOnlyStandaloneFile file)
    => JsonUtil.Deserialize<T>(file.ReadAllText());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Serialize<T>(this IStandaloneFile file, T instance)
      where T : notnull {
    using var fs = file.OpenWrite();
    using var sw = new StreamWriter(fs);
    sw.Write(JsonUtil.Serialize(instance));
    fs.SetLength(fs.Position);
  }


  // Read methods
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static StreamReader OpenReadAsText(this IReadOnlyStandaloneFile file)
    => new(file.OpenRead());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBinaryReader OpenReadAsBinary(this IReadOnlyStandaloneFile file)
    => new SchemaBinaryReader(file.OpenRead());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IBinaryReader OpenReadAsBinary(
      this IReadOnlyStandaloneFile file,
      Endianness endianness)
    => new SchemaBinaryReader(file.OpenRead(), endianness);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ReadNew<T>(this IReadOnlyStandaloneFile file)
      where T : IBinaryDeserializable, new() {
    using var br = file.OpenReadAsBinary();
    return br.ReadNew<T>();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ReadNew<T>(this IReadOnlyStandaloneFile file,
                             Endianness endianness)
      where T : IBinaryDeserializable, new() {
    using var br = file.OpenReadAsBinary(endianness);
    return br.ReadNew<T>();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T ReadNewFromText<T>(this IReadOnlyStandaloneFile file)
      where T : ITextDeserializable, new() {
    using var tr = new SchemaTextReader(file.OpenRead());
    return tr.ReadNew<T>();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte[] ReadAllBytes(this IReadOnlyStandaloneFile file) {
    using var s = file.OpenRead();
    using var ms = new MemoryStream();
    s.CopyTo(ms);
    return ms.ToArray();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static async Task<byte[]> ReadAllBytesAsync(
      this IReadOnlyStandaloneFile file) {
    await using var s = file.OpenRead();
    using var ms = new MemoryStream();
    await s.CopyToAsync(ms);
    return ms.ToArray();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ReadAllText(this IReadOnlyStandaloneFile file) {
    using var sr = file.OpenReadAsText();
    return sr.ReadToEnd();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] ReadAllLines(this IReadOnlyStandaloneFile file)
    => file.ReadAllText()
           .Split(TextReaderConstants.NEWLINE_STRINGS,
                  StringSplitOptions.None);


  // Write methods
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static StreamWriter OpenWriteAsText(this IStandaloneFile file)
    => new(file.OpenWrite());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void CopyTo(this IReadOnlyStandaloneFile src, IStandaloneFile dst) {
    using var srcStream = src.OpenRead();
    using var dstStream = dst.OpenWrite();
    srcStream.CopyTo(dstStream);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteAllBytes(this IStandaloneFile file,
                                   ReadOnlySpan<byte> bytes) {
    using var s = file.OpenWrite();
    s.Write(bytes);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteAllText(this IStandaloneFile file,
                                  string text) {
    using var sw = file.OpenWriteAsText();
    sw.Write(text);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write<T>(this IStandaloneFile file, T data)
      where T : IBinarySerializable, new() {
    using var fs = file.OpenWrite();
    using var bw = new SchemaBinaryWriter();
    data.Write(bw);
    bw.CompleteAndCopyTo(fs);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Write<T>(this IStandaloneFile file,
                              T data,
                              Endianness endianness)
      where T : IBinarySerializable, new() {
    using var fs = file.OpenWrite();
    using var bw = new SchemaBinaryWriter(endianness);
    data.Write(bw);
    bw.CompleteAndCopyTo(fs);
  }
}