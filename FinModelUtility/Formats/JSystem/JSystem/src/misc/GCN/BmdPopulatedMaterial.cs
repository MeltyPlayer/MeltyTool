using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using fin.schema;

using gx;

using jsystem.schema.j3dgraph.bmd.mat3;

namespace jsystem.GCN;

public class BmdPopulatedMaterial : IPopulatedMaterial {
  public string Name { get; set; }
  public GxCullMode CullMode { get; set; }
  public byte ColorChannelControlsCountIndex;
  public byte TexGensCountIndex;
  public byte TevStagesCountIndex;
  public byte ZCompLocIndex;
  public byte ZModeIndex;
  public byte DitherIndex;

  public (int, Color)[] MaterialColors { get; set; }
  public IColorChannelControl?[] ColorChannelControls { get; set; }
  public (int, Color)[] AmbientColors { get; set; }
  public Color?[] LightColors { get; set; }

  public Color[] KonstColors { get; set; }
  public IColorRegister?[] ColorRegisters { get; set; }


  public ushort[] TexGenInfo;

  public ushort[] TexGenInfo2;
  public ITextureMatrixInfo?[] TextureMatrices { get; set; }
  public ushort[] DttMatrices;
  public short[] TextureIndices { get; set; }
  public ushort[] TevKonstColorIndexes;
  public byte[] ConstColorSel;
  public byte[] ConstAlphaSel;

  public ITevOrder?[] TevOrderInfos { get; set; }

  public ushort[] TevOrderInfoIndexes;
  public ushort[] TevColorIndexes;
  public ITevStageProps?[] TevStageInfos { get; set; }
  public ITevSwapMode?[] TevSwapModes { get; set; }
  public ITevSwapModeTable?[] TevSwapModeTables { get; set; }

  [Unknown]
  public ushort[] Unknown2;

  public short FogInfoIndex;
  public IAlphaCompare AlphaCompare { get; set; }
  public IBlendFunction BlendMode { get; set; }

  [Unknown]
  public short UnknownIndex;

  public IDepthFunction DepthFunction { get; }

  public ITexCoordGen?[] TexCoordGens { get; set; }

  public BmdPopulatedMaterial(BMD.MAT3Section mat3, int index, MaterialEntry entry) {
      this.Name = mat3.MaterialNameTable[index];

      this.CullMode = mat3.CullModes[entry.CullModeIndex];
      this.DepthFunction = mat3.DepthFunctions[entry.DepthFunctionIndex];

      this.MaterialColors =
          entry.MaterialColorIndexes
               .Select(i => ((int) i, GetOrNull_(mat3.MaterialColor, i)))
               .ToArray();
      this.AmbientColors =
          entry.AmbientColorIndexes
               .Select(i => ((int) i, GetOrNull_(mat3.AmbientColors, i)))
               .ToArray();

      this.LightColors =
          entry.LightColorIndexes
               .Select(i => GetOrNullStruct_(mat3.LightColors, i))
               .ToArray();

      this.ColorRegisters =
          entry.TevColorIndexes
               .Select(i => {
                 var color = GetOrNull_(mat3.TevColors, i);
                 if (color != null) {
                   return (IColorRegister) new GxColorRegister { Color = color, Index = i, };
                 }

                 return null;
               })
               .ToArray();
      this.KonstColors =
          entry.TevKonstColorIndexes
               .Select(i => GetOrNull_(mat3.TevKonstColors, i))
               .ToArray();

      this.ColorChannelControls =
          entry.ColorChannelControlIndexes
               .Select(i => GetOrNull_(mat3.ColorChannelControls, i))
               .ToArray();

      this.TextureMatrices =
          entry.TexMatrices
               .Select(i => GetOrNull_(mat3.TextureMatrices, i))
               .ToArray();

      this.TevOrderInfos =
          entry.TevOrderInfoIndexes
               .Select(i => {
                 var tevOrder = GetOrNull_(mat3.TevOrders, i);
                 if (tevOrder == null) {
                   return null;
                 }

                 return new TevOrderWrapper(tevOrder) {
                   KonstAlphaSel = entry.KonstAlphaSel[i],
                   KonstColorSel = entry.KonstColorSel[i],
                 };
               })
               .ToArray();

      this.TevStageInfos =
          entry.TevStageInfoIndexes
               .Select(i => GetOrNull_(mat3.TevStages, i))
               .ToArray();

      this.TevSwapModes =
          entry.TevSwapModeInfo
               .Select(i => GetOrNull_(mat3.TevSwapModes, i))
               .ToArray();
      this.TevSwapModeTables =
          entry.TevSwapModeTable
               .Select(i => GetOrNull_(mat3.TevSwapModeTables, i))
               .ToArray();

      this.TextureIndices =
          entry.TextureIndexes
               .Select(t => (short)(t != -1 ? mat3.TextureIndices[t] : -1))
               .ToArray();

      this.TexCoordGens =
          entry.TexGenInfo
               .Select(i => GetOrNull_(mat3.TexCoordGens, i))
               .ToArray();

      this.AlphaCompare = mat3.AlphaCompares[entry.AlphaCompareIndex];
      this.BlendMode = mat3.BlendFunctions[entry.BlendModeIndex];
    }

  private class TevOrderWrapper : ITevOrder {
    private TevOrder impl_;

    public TevOrderWrapper(TevOrder impl) {
        this.impl_ = impl;
      }

    public GxTexCoord TexCoordId => this.impl_.TexCoordId;
    public GxTexMap TexMap => this.impl_.TexMap;
    public GxColorChannel ColorChannelId => this.impl_.ColorChannelId;

    public GxKonstColorSel KonstColorSel { get; set; }
    public GxKonstAlphaSel KonstAlphaSel { get; set; }
  }

  private static T? GetOrNull_<T>(IList<T> array, int i)
      where T : notnull
    => i != -1 ? array[i] : default;

  private static T? GetOrNullStruct_<T>(IList<T> array, int i)
      where T : struct
    => i != -1 ? array[i] : null;
}