using System.Numerics;

using fin.schema.color;

using schema.binary;
using schema.binary.attributes;

namespace mkdd.schema.bol;

[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class Bol : IBinaryDeserializable {
  private readonly string magic_ = "0015";
  public byte TiltSetting { get; set; }
  public Rgb24 AmbientColor { get; set; }
  public Rgba32 LightColor { get; set; }
  public Vector3 LightPosition { get; set; }
  public byte LapCount { get; set; }
  public byte CourseId { get; set; }
  private ushort routeEntryCount_;
  private ushort checkpointGroupCount_;
  private ushort objectEntryCount_;
  private ushort areaEntryCount_;
  private ushort cameraEntryCount_;
  private ushort pathEntryCount_;
  private ushort respawnEntryCount_;
  public byte FogType { get; set; }
  public Rgb24 FogColor { get; set; }
  public float FogStartZ { get; set; }
  public float FogEndZ { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool LodBiasEnabled { get; set; }

  public byte Unk0 { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool SnowEnabled { get; set; }

  public Argb32 ShadowColor { get; set; }
  private byte startingPointCount_;
  public byte SkyFollowFlag { get; set; }
  private byte lightParamEntryCount_;
  private byte mgParamEntryCount_;
  public byte Unk1 { get; set; }
  private readonly uint padding0_ = 0;
  private readonly uint routesOffset_ = 0x7C;
  private uint checkpointGroupingsOffset_;
  private uint pathsOffset_;
  private uint pathPointsOffset_;
  private uint objectsOffset_;
  private uint startingPointsOffset_;
  private uint areasOffset_;
  private uint camerasOffset_;
  private uint respawnPointsOffset_;
  private uint lightParamsOffset_;
  private uint mgParamsOffset_;

  [SequenceLengthSource(12)]
  private byte[] padding1_;

  [RAtPosition(nameof(objectsOffset_))]
  [RSequenceLengthSource(nameof(objectEntryCount_))]
  public Object[] Objects { get; set; }
}