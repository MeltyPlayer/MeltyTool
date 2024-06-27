using System.Collections.Generic;
using System.IO;

using fin.decompression;

using schema.binary;

namespace grezzo.schema.zsi;

public class Zsi : IBinaryDeserializable {
  public byte Version { get; set; }
  public string Name { get; set; }

  public List<MeshHeader> MeshHeaders { get; set; }
  public List<IEnvironmentSettings> EnvironmentSettings { get; set; }
  public List<string> RoomFileNames { get; set; }

  public void Read(IBinaryReader br) {
    var isCompressed =
        new LzssDecompressor().TryToDecompress(br, out var decompressed);
    if (isCompressed) {
      br = new SchemaBinaryReader(new MemoryStream(decompressed!),
                                  br.Endianness);
    }

    br.AssertString("ZSI");
    this.Version = br.ReadByte();

    var isMm3d = this.Version > 1;

    this.Name = br.ReadString(12);

    br.PushLocalSpace();
    {
      var commands
          = new List<(ZsiSectionType cmdType, uint cmd0, uint cmd1)>();
      while (true) {
        var cmd0 = br.ReadUInt32();
        var cmd1 = br.ReadUInt32();
        var cmdType = (ZsiSectionType) (cmd0 & 0xFF);

        commands.Add((cmdType, cmd0, cmd1));

        if (cmdType == ZsiSectionType.END) {
          break;
        }
      }

      this.MeshHeaders = new List<MeshHeader>();
      this.EnvironmentSettings = new List<IEnvironmentSettings>();
      this.RoomFileNames = new List<string>();
      foreach (var (cmdType, cmd0, cmd1) in commands) {
        switch (cmdType) {
          case ZsiSectionType.MESH_HEADER: {
            br.Position = cmd1;
            var meshHeader = br.ReadNew<MeshHeader>();
            this.MeshHeaders.Add(meshHeader);
            break;
          }
          case ZsiSectionType.ENVIRONMENT_SETTINGS: {
            var count = (cmd0 >> 8) & 0xFF;

            br.Position = cmd1;
            for (var i = 0; i < count; ++i) {
              var environmentSettings = isMm3d
                  ? (IEnvironmentSettings)
                  br.ReadNew<EnvironmentSettingsMm3d>()
                  : br.ReadNew<EnvironmentSettingsOot3d>();
              this.EnvironmentSettings.Add(environmentSettings);
            }

            break;
          }
          case ZsiSectionType.ROOMS: {
            var count = (cmd0 >> 8) & 0xFF;

            var roomStringLength = isMm3d ? 0x34 : 0x44;

            br.Position = cmd1;
            for (var i = 0; i < count; ++i) {
              this.RoomFileNames.Add(br.ReadString(roomStringLength));
            }

            break;
          }
        }
      }
    }
    br.PopLocalSpace();
  }
}