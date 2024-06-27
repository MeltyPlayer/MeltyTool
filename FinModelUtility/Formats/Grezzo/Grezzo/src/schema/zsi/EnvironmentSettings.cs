using fin.math.xyz;
using fin.schema.color;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.zsi;

[BinarySchema]
public partial class LightNormal : IReadOnlyXyz, IBinaryConvertible {
  [NumberFormat(SchemaNumberType.SN8)]
  public float X { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float Y { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float Z { get; set; }
}

public interface IEnvironmentSettings : IZsiSection {
  float DrawDistance { get; }
  public short Flags { get; }

  public Rgb24 SceneAmbientColor { get; }
  public Rgb24 ActorAmbientColor { get; }

  public Rgb24 LightColor0 { get; }
  public LightNormal LightNormal0 { get; }

  public Rgb24 LightColor1 { get; }
  public LightNormal LightNormal1 { get; }

  public float FogStart { get; }
  public float FogEnd { get; }
  public float BlendRate { get; }
  public Rgb24 FogColor { get; }
}

[BinarySchema]
public partial class EnvironmentSettingsOot3d : IEnvironmentSettings, IBinaryConvertible {
  public float DrawDistance { get; set; }
  public float FogEnd { get; set; }

  public short Flags { get; set; }

  [Skip]
  public float FogStart => this.Flags & 0x03FF;

  [Skip]
  public float BlendRate => (this.Flags >> 10) * 4;

  public Rgb24 AmbientColor { get; } = new();

  [Skip]
  public Rgb24 SceneAmbientColor => this.AmbientColor;

  [Skip]
  public Rgb24 ActorAmbientColor => this.AmbientColor;

  public Rgb24 LightColor0 { get; } = new();
  public LightNormal LightNormal0 { get; } = new();

  public Rgb24 LightColor1 { get; } = new();
  public LightNormal LightNormal1 { get; } = new();

  public Rgb24 FogColor { get; } = new();
}

[BinarySchema]
public partial class EnvironmentSettingsMm3d : IEnvironmentSettings, IBinaryConvertible {
  public Rgb24 ActorAmbientColor { get; } = new();
  public Rgb24 SceneAmbientColor { get; } = new();

  public LightNormal LightNormal0 { get; } = new();
  public Rgb24 LightColor0 { get; } = new();

  public LightNormal LightNormal1 { get; } = new();
  public Rgb24 LightColor1 { get; } = new();

  public Rgb24 FogColor { get; } = new();

  private byte unk_;

  public short Flags { get; set; }

  [Skip]
  public float FogStart => this.Flags & 0x03FF;

  [Skip]
  public float BlendRate => (this.Flags >> 10) * 4;

  public float FogEnd { get; set; }
  public float DrawDistance { get; set; }
}