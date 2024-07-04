using System;
using System.Collections.Generic;

using fin.schema;
using fin.schema.color;
using fin.schema.vector;
using fin.util.enums;

using gx;

using schema.binary;
using schema.binary.attributes;

namespace mod.schema.mod;
////////////////////////////////////////////////////////////////////
// NOTE: the names of the classes are taken directly from sysCore //
// with the exception of unknowns (_Unk)                          //
////////////////////////////////////////////////////////////////////
// Also, I am using signed types because I am unsure whether or   //
// not negative values are needed                                 //
////////////////////////////////////////////////////////////////////
// PCI = PolygonColourInfo                                        //
// TXD = TextureData                                              //
// TEV = TextureEnvironment                                       //
// TCR = Texture Environment (TEV) Colour Register                //
////////////////////////////////////////////////////////////////////

[BinarySchema]
public partial class KeyInfoU8 : IBinaryConvertible {
  [Unknown]
  public byte AnimationFrame = 0;

  [Unknown]
  public byte unknownA = 0;

  [Unknown]
  public ushort unknownB = 0;

  [Unknown]
  public float StartValue = 0;

  [Unknown]
  public float EndValue = 0;

  public string? ToString()
    => $"{this.AnimationFrame} {this.StartValue} {this.EndValue}";
}

[BinarySchema]
public partial class KeyInfoF32 : IBinaryConvertible {
  [Unknown]
  public float unknown1 = 0;

  [Unknown]
  public float unknown2 = 0;

  [Unknown]
  public float unknown3 = 0;

  public string? ToString()
    => $"{this.unknown1} {this.unknown2} {this.unknown3}";
}

[BinarySchema]
public partial class KeyInfoS10 : IBinaryConvertible {
  [Unknown]
  public short unknown1 = 0;

  public readonly short padding = 0; // TODO: Is this right?

  [Unknown]
  public float unknown2 = 0;

  [Unknown]
  public float unknown3 = 0;

  public string? ToString()
    => $"{this.unknown1} {this.unknown2} {this.unknown3}";
};

[BinarySchema]
public partial class ColorAnimationInfo : IBinaryConvertible {
  [Unknown]
  public int Index = 0;

  public readonly KeyInfoU8 R = new();
  public readonly KeyInfoU8 G = new();
  public readonly KeyInfoU8 B = new();
}

[BinarySchema]
public partial class AlphaAnimationInfo : IBinaryConvertible {
  [Unknown]
  public int Index = 0;

  public readonly KeyInfoU8 A = new();
}

[BinarySchema]
public partial class PolygonColorInfo : IBinaryConvertible {
  public Rgba32 DiffuseColour;

  [Unknown]
  public int AnimationLength = 0;

  [Unknown]
  public float AnimationSpeed = 0;

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public ColorAnimationInfo[] ColorAnimationInfo;

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public AlphaAnimationInfo[] AlphaAnimationInfo;
}

[Flags]
public enum LightingInfoFlags : uint {
  ENABLED = 0x1,
  SPECULAR_ENABLED = 0x2,
  ALPHA_ENABLED = 0x4,
  UNK_0 = 0x10,
  UNK_1 = 0x40,
  UNK_2 = 0x80,
}

[BinarySchema]
public partial class LightingInfo : IBinaryConvertible {
  public LightingInfoFlags typeFlags = 0;

  [Unknown]
  public float unknown2 = 0;
}

[BinarySchema]
public partial class PeInfo : IBinaryConvertible {
  public int Flags = 0;

  public int AlphaCompareFunctionBits { get; set; }

  [Skip]
  public GxCompareType CompareType0
    => (GxCompareType) ((AlphaCompareFunctionBits >> 0) & 0xF);

  [Skip]
  public float Reference0
    => ((AlphaCompareFunctionBits >> 4) & 0xFF) / 255f;

  [Skip]
  public GxAlphaOp AlphaCompareOp
    => (GxAlphaOp) ((AlphaCompareFunctionBits >> 16) & 0xF);

  [Skip]
  public GxCompareType CompareType1
    => (GxCompareType) ((AlphaCompareFunctionBits >> 20) & 0xF);

  [Skip]
  public float Reference1
    => ((AlphaCompareFunctionBits >> 24) & 0xFF) / 255f;


  public int ZMode = 0;

  public int BlendModeBits { get; set; }

  [Skip]
  public GxBlendMode BlendMode =>
      (GxBlendMode) ((BlendModeBits >> 0) & 0xF);

  [Skip]
  public GxBlendFactor SrcFactor =>
      (GxBlendFactor) ((BlendModeBits >> 4) & 0xF);

  [Skip]
  public GxBlendFactor DstFactor =>
      (GxBlendFactor) ((BlendModeBits >> 8) & 0xF);

  [Skip]
  public GxLogicOp LogicOp =>
      (GxLogicOp) ((BlendModeBits >> 12) & 0xF);
};

[BinarySchema]
public partial class TXD_Unk1 : IBinaryConvertible {
  [Unknown]
  public int unknown1 = 0;

  [Unknown]
  public readonly KeyInfoF32 unknown2 = new();

  [Unknown]
  public readonly KeyInfoF32 unknown3 = new();

  [Unknown]
  public readonly KeyInfoF32 unknown4 = new();
}

[BinarySchema]
public partial class TextureData : IBinaryConvertible {
  public int TexAttrIndex = 0;

  [Unknown]
  public short unknown2 = 0;

  [Unknown]
  public short unknown3 = 0;

  [Unknown]
  public byte unknown4 = 0;

  [Unknown]
  public byte unknown5 = 0;

  [Unknown]
  public byte unknown6 = 0;

  [Unknown]
  public byte unknown7 = 0;

  [Unknown]
  public uint TextureMatrixIdx = 0;

  [Unknown]
  public int AnimationLength = 0;

  [Unknown]
  public float AnimationSpeed = 0;

  [Unknown]
  public readonly Vector2f Uv = new();

  [Unknown]
  public float Rotation = 0;

  [Unknown]
  public readonly Vector2f Pivot = new();

  public readonly Vector2f Position = new();

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TXD_Unk1[] PositionAnimationData;

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TXD_Unk1[] RotationAnimationData;

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TXD_Unk1[] ScaleAnimationData;
};

public class MaterialContainer {
  public readonly List<Material> materials = [];
  public readonly List<TEVInfo> texEnvironments = [];
}

[Flags]
public enum MaterialFlags : uint {
  ENABLED = 0x1,
  OPAQUE = 0x100,
  ALPHA_CLIP = 0x200,
  TRANSPARENT_BLEND = 0x400,
  INVERT_SPECIAL_BLEND = 0x8000,
  HIDDEN = 0x10000,
}

public class Material : IBinaryConvertible {
  public MaterialFlags flags = 0;

  [Unknown]
  public uint unknown1 = 0;

  public Rgba32 SomeColor;

  public uint TevGroupId = 0;
  public readonly PolygonColorInfo ColorInfo = new();
  public readonly LightingInfo lightingInfo = new();
  public readonly PeInfo peInfo = new();
  public readonly TextureInfo texInfo = new();

  public void Read(IBinaryReader br) {
    this.flags = (MaterialFlags) br.ReadUInt32();
    this.unknown1 = br.ReadUInt32();
    this.SomeColor.Read(br);

    if (this.flags.CheckFlag(MaterialFlags.ENABLED)) {
      this.TevGroupId = br.ReadUInt32();
      this.ColorInfo.Read(br);
      this.lightingInfo.Read(br);
      this.peInfo.Read(br);
      this.texInfo.Read(br);
    }
  }

  public void Write(IBinaryWriter bw) {
    throw new NotImplementedException();
  }

  public override string ToString()
    => $"[{this.flags}] --> {this.lightingInfo.typeFlags}";
}

[BinarySchema]
public partial class TextureInfo : IBinaryConvertible {
  [Unknown]
  public int unknown1 = 0;

  [Unknown]
  public readonly Vector3f unknown2 = new();

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TexGenData[] TexGenData = [];

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TextureData[] TexturesInMaterial = [];
}

[BinarySchema]
public partial class TexGenData : IBinaryConvertible {
  public byte TexCoordId = 0;
  public GxTexGenType TexGenType = 0;
  public GxTexGenSrc TexGenSrc { get; set; }

  public GxTexMatrix TexMatrix = 0;
}

[BinarySchema]
public partial class TEVInfo : IBinaryConvertible {
  [SequenceLengthSource(3)]
  public TEVColReg[] ColorRegisters { get; set; }

  [SequenceLengthSource(4)]
  public Rgba32[] KonstColors { get; set; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TEVStage[] TevStages;
}

[BinarySchema]
public partial class TEVColReg : IBinaryConvertible {
  [Unknown]
  public readonly Rgba64 Color = new();

  [Unknown]
  public int unknown2 = 0;

  [Unknown]
  public float unknown3 = 0;

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TCR_Unk1[] unknown4;

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TCR_Unk2[] unknown5;
}

[BinarySchema]
public partial class TCR_Unk1 : IBinaryConvertible {
  [Unknown]
  public int unknown1 = 0;

  [Unknown]
  public readonly KeyInfoS10 unknown2 = new();

  [Unknown]
  public readonly KeyInfoS10 unknown3 = new();

  [Unknown]
  public readonly KeyInfoS10 unknown4 = new();

  public string? ToString()
    => $"\t\t\tUNK1: {this.unknown1}\n" +
       $"\t\t\tUNK2: {this.unknown2}\n" +
       $"\t\t\tUNK3: {this.unknown3}\n" +
       $"\t\t\tUNK4: {this.unknown4}\n";
}

[BinarySchema]
public partial class TCR_Unk2 : IBinaryConvertible {
  [Unknown]
  public int unknown1 = 0;

  [Unknown]
  public readonly KeyInfoS10 unknown2 = new();
}

[BinarySchema]
public partial class TEVStage : IBinaryConvertible {
  // TODO: This is a guess
  public byte TexCoordId { get; set; }

  // TODO: This is a guess
  public sbyte TexMap { get; set; }

  [Unknown]
  public byte unknown3 = 0;

  public GxColorChannel ColorChannel { get; set; }

  public GxKonstColorSel KonstColorSelection { get; set; }
  public GxKonstAlphaSel KonstAlphaSelection { get; set; }

  [Unknown]
  public ushort unknown65 = 0;

  public ColorCombiner ColorCombiner { get; } = new();
  public AlphaCombiner AlphaCombiner { get; } = new();
}

[BinarySchema]
public partial class ColorCombiner : IBinaryConvertible {
  public GxCc colorA = 0;
  public GxCc colorB = 0;
  public GxCc colorC = 0;
  public GxCc colorD = 0;

  public TevOp colorOp = 0;
  public TevBias colorBias = 0;
  public TevScale colorScale = 0;

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool colorClamp;

  public ColorRegister colorRegister = 0;

  [Unknown]
  public byte unknown10 = 0;

  [Unknown]
  public byte unknown11 = 0;

  [Unknown]
  public byte unknown12 = 0;
}

[BinarySchema]
public partial class AlphaCombiner : IBinaryConvertible {
  public GxCa alphaA = 0;
  public GxCa alphaB = 0;
  public GxCa alphaC = 0;
  public GxCa alphaD = 0;

  public TevOp alphaOp = 0;
  public TevBias alphaBias = 0;
  public TevScale alphaScale = 0;

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool alphaClamp;

  public ColorRegister alphaRegister = 0;

  [Unknown]
  public byte unknown10 = 0;

  [Unknown]
  public byte unknown11 = 0;

  [Unknown]
  public byte unknown12 = 0;
}