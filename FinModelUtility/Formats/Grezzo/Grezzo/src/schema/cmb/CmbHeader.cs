using fin.schema;
using fin.util.asserts;
using fin.util.strings;

using schema.binary;

namespace grezzo.schema.cmb;

public class CmbHeader : IBinaryDeserializable {
  // TODO: Better way to do this?
  public static Version Version { get; set; }

  public uint fileSize { get; private set; }
  public Version version { get; private set; }
  public string name { get; private set; }
  public uint faceIndicesCount { get; private set; }
  public uint sklOffset { get; private set; }
  public uint qtrsOffset { get; private set; }
  public uint matsOffset { get; private set; }
  public uint texOffset { get; private set; }
  public uint sklmOffset { get; private set; }
  public uint lutsOffset { get; private set; }
  public uint vatrOffset { get; private set; }
  public uint faceIndicesOffset { get; private set; }
  public uint textureDataOffset { get; private set; }

  [Unknown]
  public uint unk0 { get; private set; }

  public void Read(IBinaryReader br) {
      br.AssertString("cmb" + AsciiUtil.GetChar(0x20));

      this.fileSize = br.ReadUInt32();

      this.version = CmbHeader.Version = (Version) br.ReadUInt32();


      Asserts.Equal(0, br.ReadInt32());
      this.name = br.ReadString(16);
      this.faceIndicesCount = br.ReadUInt32();
      this.sklOffset = br.ReadUInt32();

      if (this.version.SupportsQtrs()) {
        this.qtrsOffset = br.ReadUInt32();
      }

      this.matsOffset = br.ReadUInt32();
      this.texOffset = br.ReadUInt32();
      this.sklmOffset = br.ReadUInt32();
      this.lutsOffset = br.ReadUInt32();
      this.vatrOffset = br.ReadUInt32();
      this.faceIndicesOffset = br.ReadUInt32();
      this.textureDataOffset = br.ReadUInt32();

      if (this.version > Version.OCARINA_OF_TIME_3D) {
        this.unk0 = br.ReadUInt32();
      }
    }
}