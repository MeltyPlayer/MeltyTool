using schema.binary;
using schema.binary.attributes;

namespace hw.schema.binary;

public enum BinaryResourceChunkType : ulong {
  Unknown = 0,

  XTD_XTDHeader = 0x1111,
  XTD_TerrainChunk = 0x2222,
  XTD_AtlasChunk = 0x8888,
  XTD_TessChunk = 0xAAAA,
  XTD_LightingChunk = 0xBBBB,
  XTD_AOChunk = 0xCCCC,
  XTD_AlphaChunk = 0xDDDD,

  XTT_TerrainAtlasLinkChunk = 0x2222,
  XTT_AtlasChunkAlbedo = 0x6666,
  XTT_RoadChunk = 0x8888,
  XTT_FoliageHeaderChunk = 0xAAAA,
  XTT_FoliageQNChunk = 0xBBBB,

  UGX_CachedDataChunk = 0x00000700,
  UGX_IndexBufferChunk = 0x00000701,
  UGX_VertexBufferChunk = 0x00000702,
  UGX_GrxChunk = 0x00000703, // granny_file_info
  UGX_MaterialChunk = 0x00000704,
  UGX_TreeChunk = 0x00000705,
  UGX_CachedDataSignatureHW1_BE = 0xC2340004,
  UGX_CachedDataSignatureHW1_LE = 0x040034C2,
  UGX_CachedDataSignatureHW2 = 0x060034C2, // 0xC2340006
}

[BinarySchema]
public partial class BinaryResourceChunk : IBinaryConvertible {
  public BinaryResourceChunkType Type { get; set; }
  public uint Offset { get; }
  public uint Size { get; }

  [SequenceLengthSource(2)]
  private uint[] unk0;
}