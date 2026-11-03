using System;
using System.IO;
using System.Linq;

using NoAlloq;

namespace fin.io;

public static class MagicTextUtil {
  public static bool Verify(IReadOnlyStandaloneFile file, string expected) {
    using var r = file.OpenRead();
    return Verify(r, expected);
  }

  public static bool Verify(Stream stream, string expected) {
    var tmp = stream.Position;
    var match = expected.All(c => (byte) c == stream.ReadByte());
    stream.Position = tmp;
    return match;
  }

  public static bool Verify(ReadOnlySpan<byte> data, string expected) {
    Span<byte> expectedSpan = stackalloc byte[expected.Length];
    for (var i = 0; i < expectedSpan.Length; ++i) {
      expectedSpan[i] = (byte) expected[i];
    }

    return data[..expected.Length].SequenceEqual(expectedSpan);
  }
}