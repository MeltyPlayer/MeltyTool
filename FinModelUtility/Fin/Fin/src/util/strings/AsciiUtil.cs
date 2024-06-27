using System.Text;

namespace fin.util.strings;

public static class AsciiUtil {
  private static readonly byte[] byteWrapper_ = new byte[1];
  private static readonly char[] charWrapper_ = new char[1];

  public static char GetChar(byte b) {
    AsciiUtil.byteWrapper_[0] = b;
    Encoding.ASCII.GetChars(AsciiUtil.byteWrapper_,
                            0,
                            1,
                            AsciiUtil.charWrapper_,
                            0);
    return AsciiUtil.charWrapper_[0];
  }
}