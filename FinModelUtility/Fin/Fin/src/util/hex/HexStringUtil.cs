namespace fin.util.hex;

public static class HexStringUtil {
  public static string ToHex(this byte value) => $"{value:X2}";
  public static string ToHex(this sbyte value) => $"{value:X2}";

  public static string ToHex(this ushort value) => $"{value:X4}";
  public static string ToHex(this short value) => $"{value:X4}";

  public static string ToHex(this uint value) => $"{value:X8}";
  public static string ToHex(this int value) => $"{value:X8}";
}