using System;
using System.Linq;

using fin.image;
using fin.schema.data;
using fin.util.strings;

using grezzo.schema.cmb.luts;
using grezzo.schema.cmb.mats;
using grezzo.schema.cmb.qtrs;
using grezzo.schema.cmb.skl;
using grezzo.schema.cmb.sklm;
using grezzo.schema.cmb.tex;
using grezzo.schema.cmb.vatr;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb;

[Endianness(Endianness.LittleEndian)]
public sealed class Cmb : IBinaryDeserializable {
  public readonly CmbHeader header = new();

  private const int TWEAK_AUTO_SIZE = -8;

  public PassThruStringMagicJankSizedSection<Skl> skl { get; set; }
  public readonly AutoStringMagicUInt32SizedSection<Qtrs> qtrs
      = new("qtrs");

  /// <summary>
  ///   For some reason, the size for this section is wrong? We have to just ignore it.
  /// </summary>
  public PassThruStringMagicJankSizedSection<Mats> mats { get; set; }

  public readonly AutoStringMagicUInt32SizedSection<Tex> tex
      = new("tex" + AsciiUtil.GetChar(0x20));

  public IImage[]? TextureImages { get; set; }

  public PassThruStringMagicJankSizedSection<Sklm> sklm { get; set; }

  public readonly AutoStringMagicUInt32SizedSection<Luts> luts
      = new("luts");

  public Vatr vatr { get; set; }

  public void Read(IBinaryReader br) {
    br.PushLocalSpace();
    this.header.Read(br);

    br.Position = this.header.sklOffset;
    this.skl = new("skl" + AsciiUtil.GetChar(0x20), new Skl { Version = this.header.version});
    this.skl.Read(br);

    if (this.header.version.SupportsQtrs()) {
      br.Position = this.header.qtrsOffset;
      this.qtrs.Read(br);
    }

    br.Position = this.header.matsOffset;
    this.mats = new("mats", new Mats { Version = this.header.version }) {
        TweakReadSize = TWEAK_AUTO_SIZE,
    };
    this.mats.Read(br);

    br.Position = this.header.texOffset;
    this.tex.Read(br);

    // TODO: Read this more accurately
    this.TextureImages
        = this.header.textureDataOffset == 0
            ? null
            : this.tex.Data.textures
                  .Select(t => br.SubreadAt(
                              this.header.textureDataOffset + t.DataOffset,
                              () => t.GetImageReader().ReadImage(br)))
                  .ToArray();

    br.Position = this.header.sklmOffset;
    this.sklm = new("sklm", new Sklm { Version = this.header.version });
    this.sklm.Read(br);

    br.Position = this.header.lutsOffset;
    this.luts.Read(br);

    br.Position = this.header.vatrOffset;
    this.vatr = new() { Version = this.header.version };
    this.vatr.Read(br);

    // Add face indices to primitive sets
    var sklm = this.sklm.Data;
    foreach (var shape in sklm.shapes.shapes) {
      foreach (var pset in shape.primitiveSets) {
        var primitive = pset.primitive;
        // # Always * 2 even if ubyte is used...
        br.Position = this.header.faceIndicesOffset +
                      2 * primitive.offset;

        primitive.indices = new uint[primitive.indicesCount];
        for (var i = 0; i < primitive.indicesCount; ++i) {
          switch (primitive.dataType) {
            case DataType.UByte: {
              primitive.indices[i] = br.ReadByte();
              break;
            }
            case DataType.UShort: {
              primitive.indices[i] = br.ReadUInt16();
              break;
            }
            default: {
              throw new NotImplementedException();
            }
          }
        }
      }
    }

    br.PopLocalSpace();
  }
}


/*pset.primitive.indices =
[int(readDataType(f, pset.primitive.dataType)) for _ in
range(pset.primitive.indicesCount)]*/