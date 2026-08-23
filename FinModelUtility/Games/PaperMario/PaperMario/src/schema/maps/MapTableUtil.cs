namespace pm.schema.maps;

public static class MapTableUtil {
  public const uint MAP_TABLE_OFFSET = 0x0006B450;
  public const uint AREA_TABLE_OFFSET = MAP_TABLE_OFFSET + 0x34A0;

  public static uint ConvertRamAddressToRomOffset(uint ramAddress)
    => ramAddress - 0x80090050 + MAP_TABLE_OFFSET;
}