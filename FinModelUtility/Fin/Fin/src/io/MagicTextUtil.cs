using System.IO;
using System.Linq;

namespace fin.io;

public static class MagicTextUtil {
  public static bool Verify(IReadOnlyGenericFile file, string expected) {
    using var r = file.OpenRead();
    return Verify(r, expected);
  }

  public static bool Verify(Stream stream, string expected) {
    var tmp = stream.Position;
    var match = expected.All(c => (byte) c == stream.ReadByte());
    stream.Position = tmp;
    return match;
  }
}