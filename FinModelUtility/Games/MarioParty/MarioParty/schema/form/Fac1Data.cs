using System.Numerics;

using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L420
/// </summary>
[BinarySchema]
public sealed partial class Fac1Data : IFormChunkData {
  private ushort faceCount_;

  [Unknown]
  public ushort Unk0 { get; set; }

  [Unknown]
  public uint Unk1 { get; set; }

  [RSequenceLengthSource(nameof(faceCount_))]
  public Face[] Faces { get; set; }
}

public sealed class Face : IBinaryConvertible {
  public VtxEntry[] VtxEntries { get; set; }
  public short UnkIndex { get; set; }
  public short AtrIndex { get; set; }

  [Unknown]
  public byte Unk { get; set; }

  public void Read(IBinaryReader br) {
    var faceType = br.ReadByte();

    if (faceType == 0x30) {
      br.Position += 6;
    } else {
      var vtxEntryCount = faceType is 0x35 ? 4 : 3;
      this.VtxEntries = br.ReadNews<VtxEntry>(vtxEntryCount);
    }

    this.UnkIndex = br.ReadInt16();
    this.AtrIndex = br.ReadInt16();
    this.Unk = br.ReadByte();
  }

  public void Write(IBinaryWriter bw) => throw new NotImplementedException();
}

[BinarySchema]
public sealed partial class VtxEntry : IBinaryConvertible {
  public ushort VertexIndex { get; set; }
  public short MaterialIndex { get; set; }
  public Vector2 Uv { get; set; }
}