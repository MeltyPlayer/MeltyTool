namespace gx.displayList;

public enum GxVertexAttribute : uint {
  PosMatIdx,

  Tex0MatIdx,
  Tex1MatIdx,
  Tex2MatIdx,
  Tex3MatIdx,
  Tex4MatIdx,
  Tex5MatIdx,
  Tex6MatIdx,
  Tex7MatIdx,

  Position,
  Normal,

  Color0,
  Color1,

  Tex0Coord,
  Tex1Coord,
  Tex2Coord,
  Tex3Coord,
  Tex4Coord,
  Tex5Coord,
  Tex6Coord,
  Tex7Coord,

  POS_MTX_ARRAY,
  NRM_MTX_ARRAY,
  TEX_MTX_ARRAY,
  LIGHT_ARRAY,
  NBT,
  MAX = NBT,
  NULL = 0xFF,
}