using System.Drawing;
using System.Numerics;

using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;

using fin.data.lazy;
using fin.io;
using fin.math.fixedPoint;
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
    using var br = fileBundle.MainFile.OpenReadAsBinary(Endianness.BigEndian);
    var tstlt = br.ReadNew<Tstlt>();

    var model = new ModelImpl {
        Files = fileBundle.MainFile.AsFileSet(),
        FileBundle = fileBundle,
    };

    var materialManager = model.MaterialManager;
    var thumbnailTexture =
        materialManager.CreateTexture(tstlt.Thumbnail.ToImage());
    thumbnailTexture.Name = "thumbnail";

    var faceTextures =
        materialManager.CreateTexture(tstlt.FaceTextures.ToImage());
    faceTextures.Name = "face";

    br.Position = 0x16770;
    var image2 = new Argb1555Image(32, 32 * 8);
    image2.Read(br);
    var image2Texture =
        materialManager.CreateTexture(image2.ToImage());
    image2Texture.Name = "palette";

    using var fs =
        new FinFile(
                "C:\\Users\\Ryan\\Desktop\\Mario Artist Experiments\\files\\DEFAULT.TSTLT")
            .OpenRead();
    var defaultBr = new SchemaBinaryReader(fs, Endianness.BigEndian);

    defaultBr.Position = 0x1f0c0;
    AddMesh_(model, defaultBr);

    return model;
  }

  private static void AddMesh_(
      ModelImpl model,
      IBinaryReader br) {
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

    var texturesAndMaterialAndSegmentedAddresses =
        Enumerable.Range(0, imageCount)
                  .Select(_ => {
                    var offset = br.Position;
                    var segmentedAddress =
                        (uint) (offset - 0x16770 + 0x0f000000);

                    var image = new L8Image(imageWidth, imageHeight);
                    image.Read(br);
                    return (image.ToImage(), segmentedAddress);
                  })
                  .Select(tuple => (
                              model.MaterialManager
                                   .AddSimpleTextureMaterialFromImage(
                                       tuple.Item1, $"{tuple.segmentedAddress}"), tuple.segmentedAddress))
                  .ToArray();

    var textureAndMaterialBySegmentedAddress =
        texturesAndMaterialAndSegmentedAddresses.ToDictionary(
            t => t.segmentedAddress,
            t => t.Item1);

    br.Position = baseOffset + vertexSectionOffset;

    var vertices = Enumerable.Range(0, vertexCount)
                             .Select(_ => br.ReadNew<Vertex>())
                             .ToArray();

    var skin = model.Skin;
    var lazyFinVertices =
        new LazyDictionary<(int index, ITexture texture), IReadOnlyVertex>(
            tuple => {
              var (index, texture) = tuple;

              var v = vertices[index];

              var finVertex =
                  skin.AddVertex(v.Position.X, v.Position.Y, v.Position.Z);

              var floatU = v.U / (texture.Image.Width * 32f);
              var floatV = v.V / (texture.Image.Height * 32f);

              finVertex.SetUv(floatU, floatV);

              finVertex.SetLocalNormal(v.NormalX, v.NormalY, v.NormalZ);

              return finVertex;
            });

    br.Position = baseOffset + opcodeSectionOffset;
    var parser = new SimpleF3dzex2OpcodeParser();
    var opcodes = br.Subread(
        opcodeSectionSize,
        () => {
          var opcodes = new LinkedList<IOpcodeCommand>();
          while (!br.Eof) {
            opcodes.AddLast(parser.Parse(br));
          }
          return opcodes;
        });

    var mesh = model.Skin.AddMesh();

    var activeVertices = new IReadOnlyVertex[0x20];
    (IReadOnlyMaterial, ITexture)? currentTextureAndMaterial = null;
    foreach (var opcode in opcodes) {
      switch (opcode) {
        case SetTileOpcodeCommand setTileOpcodeCommand: {
          var currentTexture = currentTextureAndMaterial.Value.Item2;
          currentTexture.WrapModeU =
              setTileOpcodeCommand.WrapModeS.AsFinWrapMode();
          currentTexture.WrapModeV =
              setTileOpcodeCommand.WrapModeT.AsFinWrapMode();
          break;
        }
        case SetTimgOpcodeCommand setTimgOpcodeCommand: {
          currentTextureAndMaterial =
              textureAndMaterialBySegmentedAddress[
                  setTimgOpcodeCommand.TextureSegmentedAddress];
          break;
        }
        case SimpleVtxOpcodeCommand vtxOpcodeCommand: {
          var currentTexture = currentTextureAndMaterial.Value.Item2;

          var correctedSegmentedAddress = vtxOpcodeCommand.SegmentedAddress -
              0x0f000000 + 0x16770;
          var relativeVertexOffset = correctedSegmentedAddress -
                                     (baseOffset + vertexSectionOffset);
          var startVertexIndex = (int) (relativeVertexOffset / 0x10);

          for (var i = 0; i < vtxOpcodeCommand.NumVerticesToLoad; ++i) {
            activeVertices[vtxOpcodeCommand.IndexToBeginStoringVertices + i] =
                lazyFinVertices[(startVertexIndex + i, currentTexture)];
          }

          break;
        }
        case Tri1OpcodeCommand tri1OpcodeCommand: {
          var material = currentTextureAndMaterial.Value.Item1;

          var triangleVertices =
              tri1OpcodeCommand.VertexIndicesInOrder
                               .Select(i => activeVertices[i])
                               .ToArray();
          var triangle = mesh.AddTriangles(triangleVertices);
          triangle.SetMaterial(material);

          break;
        }
        case Tri2OpcodeCommand tri2OpcodeCommand: {
          var material = currentTextureAndMaterial.Value.Item1;


          var triangle0Vertices =
              tri2OpcodeCommand.VertexIndicesInOrder0
                               .Select(i => activeVertices[i])
                               .ToArray();
          var triangle0 = mesh.AddTriangles(triangle0Vertices);
          triangle0.SetMaterial(material);

          var triangle1Vertices =
              tri2OpcodeCommand.VertexIndicesInOrder1
                               .Select(i => activeVertices[i])
                               .ToArray();
          var triangle1 = mesh.AddTriangles(triangle1Vertices);
          triangle1.SetMaterial(material);

          break;
        }
      }
    }
  }
}

// https://wiki.cloudmodding.com/oot/F3DZEX2#Vertex_Structure
[BinarySchema]
public partial class Vertex : IBinaryDeserializable {
  public Vector3s Position { get; } = new();

  private readonly ushort padding_ = 0;

  public short U { get; set; }
  public short V { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float NormalX { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float NormalY { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float NormalZ { get; set; }

  public byte Alpha { get; set; }
}