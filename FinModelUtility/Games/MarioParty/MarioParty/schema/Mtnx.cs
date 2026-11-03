using System.Numerics;

using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/MTNX.ts
/// </summary>
[BinarySchema]
public sealed partial class Mtnx : IBinaryDeserializable {
  private readonly string magic_ = "MTNX";

  [Unknown]
  private uint unk0_;

  private ushort trackCount_;
  public ushort FrameCount { get; private set; }

  public uint DsOffset { get; private set; }
  public uint KeyframesOffset { get; private set; }
  public uint DataOffset { get; private set; }

  [RSequenceLengthSource(nameof(trackCount_))]
  public MtnxTrack[] Tracks { get; private set; }
}

[BinarySchema]
public sealed partial class MtnxTrack : IBinaryDeserializable, IChildOf<Mtnx> {
  public Mtnx Parent { get; set; }

  private uint offset_;

  [RAtPosition(nameof(offset_))]
  public MtnxTrackData Data { get; } = new();
}

[BinarySchema]
public sealed partial class MtnxTrackData
    : IBinaryDeserializable, IChildOf<MtnxTrack> {
  public MtnxTrack Parent { get; set; }

  public byte Type { get; set; }
  public byte Dimension { get; set; }

  [Unknown]
  public byte Unk0 { get; set; }

  [Unknown]
  public byte Unk1 { get; set; }

  [Unknown]
  public ushort Unk2 { get; set; }

  private ushort frameCount_;

  [Unknown]
  public byte Unk3 { get; set; }

  public byte ObjIndex { get; set; }

  private ushort keyframesIndex_;
  private ushort dataIndex_;

  private ushort keyframeCount_;

  [Skip]
  public (ushort frame, Vector3 value)[] Keyframes { get; private set; }

  [ReadLogic]
  private void ReadData_(IBinaryReader br) {
    var keyframeTimes
        = br.SubreadAt(
            this.Parent.Parent.KeyframesOffset + 2 * this.keyframesIndex_,
            () => br.ReadUInt16s(this.keyframeCount_));
    var keyframeValues
        = br.SubreadAt(this.Parent.Parent.DataOffset + 4 * this.dataIndex_,
                       () => br.ReadVector3s(this.keyframeCount_));

    this.Keyframes = [.. keyframeTimes.Zip(keyframeValues)];
  }
}