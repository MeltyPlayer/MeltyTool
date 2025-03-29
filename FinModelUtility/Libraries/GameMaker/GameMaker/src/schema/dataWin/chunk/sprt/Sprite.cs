using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace gm.schema.dataWin.chunk.sprt;

[BinarySchema]
public partial class Sprite : IBinaryDeserializable {
  private uint nameOffset_;

  [NullTerminatedString]
  [RAtPosition(nameof(nameOffset_))]
  public string Name { get; set; }

  public uint Width { get; set; }
  public uint Height { get; set; }

  [Unknown]
  [SequenceLengthSource(18)]
  public uint[] Unk0 { get; set; }

  [Skip]
  public Frame[] Frames { get; set; }

  [ReadLogic]
  public void ReadTexturePages(IBinaryReader br) {
    var texturePageCount = br.ReadUInt32();
    this.Frames = new Frame[texturePageCount];

    for (var i = 0; i < texturePageCount; ++i) {
      var address = br.ReadUInt32();

      var tmp = br.Position;
      br.Position = address;

      this.Frames[i] = br.ReadNew<Frame>();

      br.Position = tmp;
    }
  }
}

[BinarySchema]
public partial class Frame : IBinaryDeserializable {
  public ushort X { get; set; }
  public ushort Y { get; set; }
  public ushort Width { get; set; }
  public ushort Height { get; set; }

  public ushort RenderX { get; set; }
  public ushort RenderY { get; set; }

  public ushort BoundingBoxX { get; set; }
  public ushort BoundingBoxY { get; set; }
  public ushort BoundingBoxWidth { get; set; }
  public ushort BoundingBoxHeight { get; set; }

  public ushort SheetId { get; set; }
}