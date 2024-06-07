using System.Collections.Generic;

using schema.binary;

namespace grezzo.schema.zsi {
  public class Zsi : IBinaryDeserializable {
    public byte Version { get; set; }
    public string Name { get; set; }
    public List<MeshHeader> MeshHeaders { get; set; }

    public void Read(IBinaryReader br) {
      br.AssertString("ZSI");
      this.Version = br.ReadByte();

      this.Name = br.ReadString(12);

      var commands = new List<(ZsiSectionType cmdType, uint cmd0, uint cmd1)>();
      while (true) {
        var cmd0 = br.ReadUInt32();
        var cmd1 = br.ReadUInt32();
        var cmdType = (ZsiSectionType) (cmd0 >> 24);

        commands.Add((cmdType, cmd0, cmd1));

        if (cmdType == ZsiSectionType.END) {
          break;
        }
      }

      this.MeshHeaders = new List<MeshHeader>();
      foreach (var (cmdType, cmd0, cmd1) in commands) {
        if (cmdType == ZsiSectionType.MESH_HEADER) {
          var meshHeader = new MeshHeader(cmd1);
          meshHeader.Read(br);

          this.MeshHeaders.Add(meshHeader);
        }
      }
    }
  }
}