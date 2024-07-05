using System;
using System.Collections.Generic;
using System.Linq;

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
public partial class PolygonColorInfo
    : IBinaryConvertible, IEquatable<PolygonColorInfo> {
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

  public bool Equals(PolygonColorInfo? other) {
    if (ReferenceEquals(null, other)) {
      return false;
    }

    if (ReferenceEquals(this, other)) {
      return true;
    }

    return this.DiffuseColour.Equals(other.DiffuseColour) &&
           this.AnimationLength == other.AnimationLength &&
           this.AnimationSpeed.Equals(other.AnimationSpeed);
  }

  public override bool Equals(object? obj) {
    if (ReferenceEquals(null, obj)) {
      return false;
    }

    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj.GetType() != this.GetType()) {
      return false;
    }

    return Equals((PolygonColorInfo) obj);
  }

  public override int GetHashCode() {
    return HashCode.Combine(this.DiffuseColour,
                            this.AnimationLength,
                            this.AnimationSpeed,
                            this.ColorAnimationInfo,
                            this.AlphaAnimationInfo);
  }
}

public enum LightingInfoFlags : uint {
  ENABLED = 1 << 0,
  SPECULAR_ENABLED = 1 << 1,
  ALPHA_ENABLED = 1 << 2,
  UNK_0 = 0x10,
  UNK_1 = 0x40,
  UNK_2 = 0x80,
  // TODO: Might be wrong
  USE_VERTEX_COLOR = 1 << 11,
  USE_VERTEX_ALPHA = 1 << 12,
}

[BinarySchema]
public partial record LightingInfo : IBinaryConvertible {
  public LightingInfoFlags typeFlags = 0;

  [Unknown]
  public float unknown2 = 0;
}

[BinarySchema]
public partial record PeInfo : IBinaryConvertible {
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


  public int ZModeBits = 0;

  [Skip]
  public bool Enable => ((ZModeBits >> 8) & 0x1) == 0x1;

  [Skip]
  public bool WriteNewIntoBuffer => ((ZModeBits >> 9) & 0x1) == 0x1;

  [Skip]
  public GxCompareType DepthCompareType
    => (GxCompareType) ((ZModeBits >> 0) & 0xF);

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
public partial class TextureData
    : IBinaryConvertible, IEquatable<TextureData> {
  public int TexAttrIndex = 0;

  [IntegerFormat(SchemaIntegerType.UINT16)]
  public GxWrapMode WrapModeS { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT16)]
  public GxWrapMode WrapModeT { get; set; }

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
  public readonly Vector2f Scale = new();

  [Unknown]
  public float Rotation = 0;

  [Unknown]
  public readonly Vector2f Position = new();

  public readonly Vector2f Pivot = new();

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TXD_Unk1[] PositionAnimationData;

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TXD_Unk1[] RotationAnimationData;

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TXD_Unk1[] ScaleAnimationData;

  public bool Equals(TextureData? other) {
    if (ReferenceEquals(null, other)) {
      return false;
    }

    if (ReferenceEquals(this, other)) {
      return true;
    }

    return this.TexAttrIndex == other.TexAttrIndex &&
           this.WrapModeS == other.WrapModeS &&
           this.WrapModeT == other.WrapModeT &&
           this.unknown4 == other.unknown4 &&
           this.unknown5 == other.unknown5 &&
           this.unknown6 == other.unknown6 &&
           this.unknown7 == other.unknown7 &&
           this.TextureMatrixIdx == other.TextureMatrixIdx &&
           this.AnimationLength == other.AnimationLength &&
           this.AnimationSpeed.Equals(other.AnimationSpeed) &&
           this.Scale.Equals(other.Scale) &&
           this.Rotation.Equals(other.Rotation) &&
           this.Position.Equals(other.Position) &&
           this.Pivot.Equals(other.Pivot);
  }

  public override bool Equals(object? obj) {
    if (ReferenceEquals(null, obj)) {
      return false;
    }

    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj.GetType() != this.GetType()) {
      return false;
    }

    return Equals((TextureData) obj);
  }

  public override int GetHashCode() {
    var hashCode = new HashCode();
    hashCode.Add(this.TexAttrIndex);
    hashCode.Add(this.WrapModeS);
    hashCode.Add(this.WrapModeT);
    hashCode.Add(this.unknown4);
    hashCode.Add(this.unknown5);
    hashCode.Add(this.unknown6);
    hashCode.Add(this.unknown7);
    hashCode.Add(this.TextureMatrixIdx);
    hashCode.Add(this.AnimationLength);
    hashCode.Add(this.AnimationSpeed);
    hashCode.Add(this.Scale);
    hashCode.Add(this.Rotation);
    hashCode.Add(this.Position);
    hashCode.Add(this.Pivot);
    hashCode.Add(this.PositionAnimationData);
    hashCode.Add(this.RotationAnimationData);
    hashCode.Add(this.ScaleAnimationData);
    return hashCode.ToHashCode();
  }
}

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

public record Material : IBinaryConvertible {
  public MaterialFlags flags = 0;

  [Unknown]
  public int unknown1 = 0;

  public Rgba32 SomeColor;

  public uint TevGroupId = 0;
  public readonly PolygonColorInfo ColorInfo = new();
  public readonly LightingInfo lightingInfo = new();
  public readonly PeInfo peInfo = new();
  public readonly TextureInfo texInfo = new();

  public void Read(IBinaryReader br) {
    this.flags = (MaterialFlags) br.ReadUInt32();
    this.unknown1 = br.ReadInt32();
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
public partial class TextureInfo : IBinaryConvertible, IEquatable<TextureInfo> {
  [Unknown]
  public int unknown1 = 0;

  [Unknown]
  public readonly Vector3f unknown2 = new();

  [Unknown]
  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TexGenData[] TexGenData = [];

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public TextureData[] TexturesInMaterial = [];

  public bool Equals(TextureInfo? other) {
    if (ReferenceEquals(null, other)) {
      return false;
    }

    if (ReferenceEquals(this, other)) {
      return true;
    }

    return this.unknown1 == other.unknown1 &&
           this.unknown2.Equals(other.unknown2) &&
           this.TexGenData.SequenceEqual(other.TexGenData) &&
           this.TexturesInMaterial.SequenceEqual(other.TexturesInMaterial);
  }

  public override bool Equals(object? obj) {
    if (ReferenceEquals(null, obj)) {
      return false;
    }

    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj.GetType() != this.GetType()) {
      return false;
    }

    return Equals((TextureInfo) obj);
  }

  public override int GetHashCode() {
    return HashCode.Combine(this.unknown1,
                            this.unknown2,
                            this.TexGenData,
                            this.TexturesInMaterial);
  }
}

[BinarySchema]
public partial record TexGenData : IBinaryConvertible {
  public GxTexCoord TexCoordId = 0;
  public GxTexGenType TexGenType = 0;
  public GxTexGenSrc TexGenSrc { get; set; }

  public byte TexMatrix = 0;
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
  [Unknown]
  public byte Unknown = 0;

  public GxTexCoord TexCoord { get; set; }
  public GxTexMap TexMap { get; set; }

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