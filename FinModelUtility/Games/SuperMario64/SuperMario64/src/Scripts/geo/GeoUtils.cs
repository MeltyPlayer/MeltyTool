namespace sm64.scripts.geo;

public static class GeoUtils {
  public static bool IsDisplayListAndDrawingLayerEnabled(byte param)
    => (param & 0x80) != 0;

  public static GeoDrawingLayer GetDrawingLayerFromParams(byte param)
    => (GeoDrawingLayer)
        (IsDisplayListAndDrawingLayerEnabled(param) ? param & 0xF : 0);

  public static GeoTranslateAndRotateFormat GetTranslateAndRotateFormat(
      byte param)
    => (GeoTranslateAndRotateFormat)((param & 0x70) >> 4);
}