using fin.image;
using fin.schema;

using schema.binary;

namespace modl.schema.res.texr;

public class Text : BTexr, ITexr {
  public IImage Image { get; private set; }

  [Unknown]
  public void Read(IBinaryReader br) {
    SectionHeaderUtil.AssertNameAndReadSize(br, "TEXT", out _);

    var textureName = br.ReadString(0x10);

    var width = br.ReadUInt32();
    var height = br.ReadUInt32();

    var unknowns0 = br.ReadUInt32s(2);

    var textureType = br.ReadString(8);
    var drawType = br.ReadString(8);

    var unknowns1 = br.ReadUInt32s(8);

    var unknowns2 = br.ReadUInt32s(1);

    br.PushMemberEndianness(Endianness.BigEndian);
    this.Image = textureType switch {
        "A8R8G8B8" => this.ReadA8R8G8B8_(br, width, height),
        "DXT1"     => this.ReadDxt1_(br, width, height),
        "P8"       => this.ReadP8_(br, width, height),
        _          => throw new NotImplementedException(),
    };
    br.PopEndianness();
  }
}