using schema.binary;

namespace dk64.schema.map;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/DonkeyKong64/scenes.ts#L587
/// </summary>
public sealed class Map : IBinaryDeserializable {
  public uint DlStart { get; set; }
  public uint DlEnd { get; set; }
  public uint VertStart { get; set; }
  public uint VertEnd { get; set; }

  public MapChunk[] MapChunks { get; set; }
  public MapSection[] MapSections { get; set; }

  public void Read(IBinaryReader br) {
    br.Position = 0x34;
    this.DlStart = br.ReadUInt32();
    this.VertStart = this.DlEnd = br.ReadUInt32();
    br.ReadUInt32();
    this.VertEnd = br.ReadUInt32();

    br.Position = 0x58;
    var sectionStart = br.ReadUInt32();
    br.ReadUInt32();
    var sectionEnd = br.ReadUInt32();

    br.Position = 0x64;
    var chunkCountOffset = br.ReadUInt32();
    var chunkStart = br.ReadUInt32();

    br.Position = chunkCountOffset;
    var chunkCount = br.ReadInt32();
    br.Position = chunkStart;
    this.MapChunks = br.ReadNews<MapChunk>(chunkCount);

    br.Position = sectionStart + 4;
    var sections = new List<MapSection>();
    while (br.Position < sectionEnd) {
      sections.Add(br.ReadNew<MapSection>());
    }

    this.MapSections = sections.ToArray();
  }
}