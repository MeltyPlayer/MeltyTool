using System;
using System.Collections.Generic;

using fin.schema;
using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

namespace mod.schema.mod.collision;

[BinarySchema]
public partial class BaseRoomInfo : IBinaryConvertible {
  public uint index = 0;
}

[BinarySchema]
public partial class BaseCollTriInfo : IBinaryConvertible {
  public uint mapCode = 0;
  public readonly Vector3i indice = new();

  [Unknown]
  public ushort unknown2 = 0;

  [Unknown]
  public ushort unknown3 = 0;

  [Unknown]
  public ushort unknown4 = 0;

  [Unknown]
  public ushort unknown5 = 0;

  public readonly Plane plane = new();
}

[BinarySchema]
public partial class CollTriInfo : IBinaryConvertible {
  [WLengthOfSequence(nameof(collinfo))]
  private uint colInfoCount_;

  [WLengthOfSequence(nameof(roominfo))]
  private uint roomInfoCount_;

  [RSequenceLengthSource(nameof(roomInfoCount_))]
  [Align(0x20)]
  public readonly List<BaseRoomInfo> roominfo = [];

  [RSequenceLengthSource(nameof(colInfoCount_))]
  [Align(0x20)]
  public readonly List<BaseCollTriInfo> collinfo = [];

  [Align(0x20)]
  private readonly byte[] empty_ = new byte[0];
}

[BinarySchema]
public partial class CollGroup : IBinaryConvertible {
  [Unknown]
  [WLengthOfSequence(nameof(unknown1))]
  private ushort NumUnknown1 { get; set; }

  [Unknown]
  [WLengthOfSequence(nameof(unknown2))]
  private ushort NumUnknown2 { get; set; }

  [Unknown]
  [RSequenceLengthSource(nameof(NumUnknown2))]
  public uint[] unknown2 = Array.Empty<uint>();

  [Unknown]
  [RSequenceLengthSource(nameof(NumUnknown1))]
  public byte[] unknown1 = Array.Empty<byte>();
}

[BinarySchema]
public partial class CollGrid : IBinaryConvertible {
  [Align(0x20)]
  public readonly Vector3f boundsMin = new();

  public readonly Vector3f boundsMax = new();

  [Unknown]
  public float unknown1 = 0;

  public uint gridX = 0;
  public uint gridY = 0;

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public readonly List<CollGroup> groups = [];

  [Skip]
  private uint gridSize_ => this.gridX * this.gridY;

  [Unknown]
  [RSequenceLengthSource(nameof(gridSize_))]
  public readonly List<int> unknown2 = [];

  [Align(0x20)]
  private readonly byte[] empty_ = new byte[0];
}