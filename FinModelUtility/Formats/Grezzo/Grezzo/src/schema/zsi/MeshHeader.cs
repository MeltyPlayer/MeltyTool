using schema.binary;

namespace grezzo.schema.zsi {
  public class MeshHeader(uint pos) : IBinaryDeserializable {
    public byte Type { get; set; }
    public byte EntryCount { get; set; }
    public short Unk { get; set; }

    public uint EntryOffset { get; set; }
    public uint UnkOffset { get; set; }

    public MeshEntry[] MeshEntries { get; set; }
    public ushort UnkValue { get; set; }

    public void Read(IBinaryReader br) {
      br.Position = pos;

      this.Type = br.ReadByte();
      this.EntryCount = br.ReadByte();
      this.Unk = br.ReadInt16();

      this.EntryOffset = br.ReadUInt32();
      this.UnkOffset = br.ReadUInt32();

      this.MeshEntries = br.SubreadAt(
          pos + this.EntryOffset,
          sbr => sbr.ReadNews<MeshEntry>(this.EntryCount));

      this.UnkValue = br.SubreadUInt16At(this.UnkOffset + 16);
    }
  }
}