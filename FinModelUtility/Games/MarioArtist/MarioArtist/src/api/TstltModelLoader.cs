using System.Drawing;

using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.io;

using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.schema.vector;
using fin.util.sets;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

public record TstltModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public partial class TstltModelLoader : IModelImporter<TstltModelFileBundle> {
  public IModel Import(TstltModelFileBundle fileBundle) {
    var tstlt = fileBundle.MainFile.ReadNew<Tstlt>();

    var model = new ModelImpl {
        Files = fileBundle.MainFile.AsFileSet(),
        FileBundle = fileBundle,
    };

    var materialManager = model.MaterialManager;
    var thumbnailTexture =
        materialManager.CreateTexture(tstlt.Thumbnail.ToImage());
    thumbnailTexture.Name = "thumbnail";

    var imageTexture =
        materialManager.CreateTexture(tstlt.FaceTextures.ToImage());
    imageTexture.Name = "face";

    using var fs =
        new FinFile("C:\\Users\\Ryan\\Desktop\\Mario Artist Experiments\\files\\DEFAULT.TSTLT")
            .OpenRead();
    var br = new SchemaBinaryReader(fs, Endianness.BigEndian);

    var n64Memory = new N64Memory([]);
    var displayListReader = new DisplayListReader();
    var parseOpcode =
        () => (IOpcodeCommand) null; // opcodeParser.Parse(n64Memory, displayListReader, br

    br.Position = 0x1f0c0;
    AddMesh_(model, br, parseOpcode);

    return model;
  }

  private static void AddMesh_(
      ModelImpl model,
      IBinaryReader br,
      Func<IOpcodeCommand> parseOpcode) {
    var baseOffset = br.Position;

    br.Position = baseOffset + 4 * 2;
    var imageSectionSize = br.ReadUInt32();
    var vertexSectionSize = br.ReadUInt32();
    var opcodeSectionSize = br.ReadUInt32();

    br.Position = baseOffset + 4 * 7;
    var imageSectionOffset = br.ReadUInt32();
    var vertexSectionOffset = br.ReadUInt32();
    var opcodeSectionOffset = br.ReadUInt32();

    br.Position = baseOffset + 4 * 14;
    var imageCount = br.ReadUInt16();
    var vertexCount = br.ReadUInt16();

    
    br.Position = baseOffset + imageSectionOffset;

    var singleImageSize = imageSectionSize / imageCount;
    var imageWidth = 16;
    var imageHeight = (int) (singleImageSize / imageWidth);

    var textures =
        Enumerable.Range(0, imageCount)
                  .Select(_ => {
                    var image = new L8Image(imageWidth, imageHeight);
                    image.Read(br);
                    return image.ToImage();
                  })
                  .Select(image => model.MaterialManager.CreateTexture(image))
                  .ToArray();

    
    br.Position = baseOffset + vertexSectionOffset;

    var vertices = Enumerable.Range(0, vertexCount)
                             .Select(_ => br.ReadNew<Vertex>())
                             .ToArray();

    var skin = model.Skin;
    var finVertices =
        vertices.Select(v => skin.AddVertex(v.Position.X,
                                            v.Position.Y,
                                            v.Position.Z))
                .ToArray();
    var points = skin.AddMesh().AddLineStrip(finVertices);
    points.SetLineWidth(100);
    points.SetMaterial(model.MaterialManager.AddColorMaterial(Color.Red));


    br.Position = baseOffset + opcodeSectionOffset;
    var opcodes = br.Subread(
        opcodeSectionSize,
        () => {
          var opcodes = new LinkedList<IOpcodeCommand>();
          while (!br.Eof) {
            opcodes.AddLast(parseOpcode());
          }
          return opcodes;
        });

    ;
  }
}

[BinarySchema]
public partial class Vertex : IBinaryDeserializable {
  public Vector3s Position { get; } = new();

  [SequenceLengthSource(10)]
  public byte[] Unk;
}