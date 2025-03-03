using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using gx;

using jsystem.G3D_Binary_File_Format;
using jsystem.schema.j3dgraph.bmd;
using jsystem.schema.j3dgraph.bmd.mat3;

using schema.binary;

#pragma warning disable CS8604


namespace jsystem.GCN;

public partial class BMD {
  public partial class MAT3Section {
    public const string Signature = "MAT3";
    public DataBlockHeader Header;
    public ushort NrMaterials;
    public uint[] Offsets;
    public MaterialEntry[] MaterialEntries;
    public BmdPopulatedMaterial[] PopulatedMaterials { get; set; }
    public ushort[] MaterialEntryIndieces;
    public short[] TextureIndices;
    public GxCullMode[] CullModes;
    public Color[] MaterialColor;
    public Color[] LightColors;
    public Color[] AmbientColors;
    public Color[] TevColors;
    public Color[] TevKonstColors;
    public AlphaCompare[] AlphaCompares;
    public BlendFunction[] BlendFunctions;
    public DepthFunction[] DepthFunctions;
    public TevStageProps[] TevStages;
    public TevSwapMode[] TevSwapModes;
    public TevSwapModeTable[] TevSwapModeTables;
    public TexCoordGen[] TexCoordGens;
    public ColorChannelControl[] ColorChannelControls;
    public TextureMatrixInfo[] TextureMatrices;
    public TevOrder[] TevOrders;
    public StringTable MaterialNameTable;

    public readonly List<IndirectTexture> IndirectTextures = [];

    public MAT3Section(IBinaryReader br, out bool OK) {
      long position1 = br.Position;
      bool OK1;
      this.Header = new DataBlockHeader(br, "MAT3", out OK1);
      if (!OK1) {
        OK = false;
      } else {
        this.NrMaterials = br.ReadUInt16();

        br.AssertUInt16(0xffff); // padding

        this.Offsets = br.ReadUInt32s(30);
        int[] sectionLengths = this.GetSectionLengths();
        long position2 = br.Position;

        // TODO: There is a bunch more data that isn't even read yet:
        // https://github.com/RenolY2/SuperBMD/blob/ccc86e21493275bcd9d86f65b516b85d95c83abd/SuperBMDLib/source/Materials/Enums/Mat3OffsetIndex.cs

        br.Position = position1 + (long) this.Offsets[0];
        this.MaterialEntries = new MaterialEntry[sectionLengths[0] / 332];
        for (int index = 0; index < sectionLengths[0] / 332; ++index)
          this.MaterialEntries[index] = br.ReadNew<MaterialEntry>();

        br.Position = position1 + (long) this.Offsets[1];
        this.MaterialEntryIndieces = br.ReadUInt16s((int) this.NrMaterials);

        br.Position = position1 + (long) this.Offsets[2];
        this.MaterialNameTable = br.ReadNew<StringTable>();

        var indirectTexturesOffset =
            br.Position = position1 + this.Offsets[3];
        this.IndirectTextures.Clear();
        while ((br.Position - indirectTexturesOffset) < sectionLengths[3]) {
          this.IndirectTextures.Add(
              br.ReadNew<IndirectTexture>());
        }

        br.Position = position1 + (long) this.Offsets[4];
        this.CullModes = new GxCullMode[sectionLengths[4] / 4];
        for (var index = 0; index < sectionLengths[4] / 4; ++index)
          this.CullModes[index] = (GxCullMode) br.ReadInt32();

        br.Position = position1 + (long) this.Offsets[5];
        this.MaterialColor = new Color[sectionLengths[5] / 4];
        for (int index = 0; index < sectionLengths[5] / 4; ++index)
          this.MaterialColor[index] = br.ReadColor8();

        br.Position = position1 + (long) this.Offsets[7];
        this.ColorChannelControls =
            new ColorChannelControl[sectionLengths[7] / 8];
        for (var i = 0; i < this.ColorChannelControls.Length; ++i) {
          this.ColorChannelControls[i] = br.ReadNew<ColorChannelControl>();
        }

        br.Position = position1 + (long) this.Offsets[8];
        this.AmbientColors = new Color[sectionLengths[8] / 4];
        for (int index = 0; index < sectionLengths[8] / 4; ++index)
          this.AmbientColors[index] = br.ReadColor8();

        br.Position = position1 + this.Offsets[9];
        this.LightColors = new Color[sectionLengths[9] / 8];
        for (int index = 0; index < this.LightColors.Length; ++index) {
          this.LightColors[index] = br.ReadColor16();
        }

        // TODO: Add support for texgen counts (10)

        br.Position = position1 + this.Offsets[11];
        this.TexCoordGens = br.ReadNews<TexCoordGen>(sectionLengths[11] / 4);

        // TODO: Add support for post tex coord gens (12)

        br.Position = position1 + (long) this.Offsets[13];
        this.TextureMatrices =
            br.ReadNews<TextureMatrixInfo>(sectionLengths[13] / 100);

        // TODO: Add support for post tex matrices (14)

        br.Position = position1 + (long) this.Offsets[15];
        this.TextureIndices = br.ReadInt16s(sectionLengths[15] / 2);

        br.Position = position1 + (long) this.Offsets[16];
        this.TevOrders = br.ReadNews<TevOrder>(sectionLengths[16] / 4);

        br.Position = position1 + (long) this.Offsets[17];
        this.TevColors = new Color[sectionLengths[17] / 8];
        for (int index = 0; index < this.TevColors.Length; ++index)
          this.TevColors[index] = br.ReadColor16();

        br.Position = position1 + (long) this.Offsets[18];
        this.TevKonstColors = new Color[sectionLengths[18] / 4];
        for (int index = 0; index < this.TevKonstColors.Length; ++index)
          this.TevKonstColors[index] = br.ReadColor8();

        // TODO: Add support for tev counts (19)

        br.Position = position1 + (long) this.Offsets[20];
        this.TevStages = br.ReadNews<TevStageProps>(sectionLengths[20] / 20);

        br.Position = position1 + (long) this.Offsets[21];
        this.TevSwapModes = br.ReadNews<TevSwapMode>(sectionLengths[21] / 4);

        br.Position = position1 + (long) this.Offsets[22];
        this.TevSwapModeTables =
            br.ReadNews<TevSwapModeTable>(sectionLengths[22] / 4);

        // TODO: Add support for fog modes (23)

        br.Position = position1 + (long) this.Offsets[24];
        this.AlphaCompares = br.ReadNews<AlphaCompare>(sectionLengths[24] / 8);

        br.Position = position1 + (long) this.Offsets[25];
        this.BlendFunctions =
            br.ReadNews<BlendFunction>(sectionLengths[25] / 4);

        br.Position = position1 + (long) this.Offsets[26];
        this.DepthFunctions =
            br.ReadNews<DepthFunction>(sectionLengths[26] / 4);

        br.Position = position1 + (long) this.Header.size;
        OK = true;

        // TODO: Add support for nbt scale (29)

        this.PopulatedMaterials = this.MaterialEntries
                                      .Select(
                                          (entry, index)
                                              => new BmdPopulatedMaterial(
                                                  this,
                                                  index,
                                                  entry))
                                      .ToArray();
      }
    }

    public int[] GetSectionLengths() {
      int[] numArray = new int[30];
      for (int index1 = 0; index1 < 30; ++index1) {
        int num1 = 0;
        if (this.Offsets[index1] != 0U) {
          int num2 = (int) this.Header.size;
          for (int index2 = index1 + 1; index2 < 30; ++index2) {
            if (this.Offsets[index2] != 0U) {
              num2 = (int) this.Offsets[index2];
              break;
            }
          }

          num1 = num2 - (int) this.Offsets[index1];
        }

        numArray[index1] = num1;
      }

      return numArray;
    }
  }
}