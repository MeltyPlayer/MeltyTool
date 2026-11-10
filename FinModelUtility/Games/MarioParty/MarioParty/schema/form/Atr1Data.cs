using fin.schema;

using schema.binary;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L372
/// </summary>
public sealed class Atr1Data : IFormChunkData {
  public Attribute[] Attributes { get; set; }

  public void Read(IBinaryReader br) {
    this.Attributes = new Attribute[br.ReadUInt16()];
    for (var i = 0; i < this.Attributes.Length; ++i) {
      var baseOffset = br.Position;
      this.Attributes[i] = br.ReadNew<Attribute>();
      br.Position = baseOffset + this.Attributes[i].Size + 2;
    }
  }

  public void Write(IBinaryWriter bw) => throw new NotImplementedException();
}

public enum WrapBehavior : byte {
  MIRRORED_REPEAT = 0x2c,
  REPEAT = 0x2d,
  CLAMP = 0x2e,
}

[BinarySchema]
public sealed partial class Attribute : IFormChunkData {
  public ushort Size { get; set; }

  [Unknown]
  public byte Unk0 { get; set; }

  [Unknown]
  public ushort Unk1 { get; set; }

  public WrapBehavior XBehavior { get; set; }
  public WrapBehavior YBehavior { get; set; }

  [Unknown]
  public byte Unk2 { get; set; }

  [Unknown]
  public ushort Unk3 { get; set; }

  public ushort BmpGlobalIndex { get; set; }
}