using fin.schema;

using gx;

using schema.binary;


namespace jsystem.schema.j3dgraph.bmd.mat3;

public enum RenderOrder : byte {
  DRAW_ON_WAY_DOWN = 1,
  DRAW_ON_WAY_UP = 4,
}

/// <summary>
///   https://github.com/LordNed/WindEditor/wiki/BMD-and-BDL-Model-Format#material-entry
/// </summary>
[BinarySchema]
public partial class MaterialEntry : IBinaryConvertible {
  public RenderOrder RenderOrder { get; set; }
  public byte CullModeIndex { get; set; }
  public byte ColorChannelControlsCountIndex { get; set; }
  public byte TexGensCountIndex { get; set; }
  public byte TevStagesCountIndex { get; set; }
  public byte ZCompLocIndex { get; set; }
  public sbyte DepthFunctionIndex { get; set; }
  public byte DitherIndex { get; set; }

  public short[] MaterialColorIndexes { get; } = new short[2];
  public ushort[] ColorChannelControlIndexes { get; } = new ushort[4];
  public ushort[] AmbientColorIndexes { get; } = new ushort[2];
  public short[] LightColorIndexes { get; } = new short[8];

  public short[] TexGenInfo { get; } = new short[8];

  public ushort[] TexGenInfo2 { get; } = new ushort[8];
  public short[] TexMatrices { get; } = new short[10];
  public ushort[] DttMatrices { get; } = new ushort[20];
  public short[] TextureIndexes { get; } = new short[8];
  public short[] TevKonstColorIndexes { get; } = new short[4];
  public GxKonstColorSel[] KonstColorSel { get; } = new GxKonstColorSel[16];
  public GxKonstAlphaSel[] KonstAlphaSel { get; } = new GxKonstAlphaSel[16];
  public short[] TevOrderInfoIndexes { get; } = new short[16];
  public short[] TevColorIndexes { get; } = new short[4];
  public short[] TevStageInfoIndexes { get; } = new short[16];
  public short[] TevSwapModeInfo { get; } = new short[16];
  public short[] TevSwapModeTable { get; } = new short[4];

  [Unknown]
  public ushort[] Unknown2 { get; } = new ushort[12];

  public short FogInfoIndex;
  public short AlphaCompareIndex;
  public short BlendModeIndex;

  [Unknown]
  public short UnknownIndex;
}