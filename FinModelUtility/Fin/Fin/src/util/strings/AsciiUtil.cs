using System.Text;

namespace fin.util.strings;

public static class AsciiUtil {
  private static readonly byte[] byteWrapper_ = new byte[1];
  private static readonly char[] charWrapper_ = new char[1];

  public static char GetChar(byte b) {
    byteWrapper_[0] = b;
    Encoding.ASCII.GetChars(byteWrapper_,
                            0,
                            1,
                            charWrapper_,
                            0);
    return charWrapper_[0];
  }
}