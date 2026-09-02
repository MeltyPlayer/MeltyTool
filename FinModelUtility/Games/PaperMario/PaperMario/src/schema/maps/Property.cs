using fin.util.types;

using schema.binary;
using schema.binary.attributes;

namespace pm.schema.maps;

public enum PropertyType : byte {
  INT,
  FLOAT,
  STRING,
}

public enum PropertyId : uint {
  TEX_ENV_NAME = 0x5E,
  UNK_5F = 0x5F,
  BOUNDING_BOX = 0x61,
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/6b16cfda00ef5af3ee2a66d8b928bb0bf700e5b6/src/PaperMario64/map_shape.ts#L82
/// </summary>
[BinarySchema]
public sealed partial class Property : IBinaryDeserializable {
  public PropertyId Id { get; set; }
  public uint Value0 { get; set; }

  [Skip]
  public PropertyType PropertyType
    => this.Id == PropertyId.UNK_5F
        ? PropertyType.INT
        : (PropertyType) (this.Value0 & 0xFF);

  [Skip]
  public IPropertyValue? Value
    => (IPropertyValue?) this.intProperty_ ??
       (IPropertyValue?) this.floatProperty_ ??
       (IPropertyValue?) this.stringProperty_;

  [Skip]
  private bool IsInt_ => this.PropertyType is PropertyType.INT;

  [RIfBoolean(nameof(IsInt_))]
  private IntProperty? intProperty_;

  [Skip]
  private bool IsFloat_ => this.PropertyType is PropertyType.FLOAT;

  [RIfBoolean(nameof(IsFloat_))]
  private FloatProperty? floatProperty_;

  [Skip]
  private bool IsString_ => this.PropertyType is PropertyType.STRING;

  [RIfBoolean(nameof(IsString_))]
  private StringProperty? stringProperty_;

  public override string ToString() => $"{this.Id}: {this.Value}";
}

[UnionCandidate]
public interface IPropertyValue : IBinaryDeserializable;

[BinarySchema]
public sealed partial class IntProperty : IPropertyValue {
  public uint Value { get; set; }
  public override string ToString() => $"{this.Value}";
}

[BinarySchema]
public sealed partial class FloatProperty : IPropertyValue {
  public float Value { get; set; }
  public override string ToString() => $"{this.Value}";
}

[BinarySchema]
public sealed partial class StringProperty : IPropertyValue {
  public uint StringRamAddress { get; set; }

  [Skip]
  private uint StringOffset => Shape.ConvertRamAddressToOffset(this.StringRamAddress);

  [RAtPositionOrNull(nameof(StringOffset))]
  [StringLengthSource(0x30)]
  public string? Value { get; set; }

  public override string ToString() => $"{this.Value}";
}