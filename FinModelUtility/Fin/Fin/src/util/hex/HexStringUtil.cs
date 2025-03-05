namespace fin.util.hex;

public static class HexStringUtil {
  public static string ToHex(this uint value) => $"{value:X8}";
}